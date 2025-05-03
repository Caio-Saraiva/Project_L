using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using URandom = UnityEngine.Random;

public class CircuitManager : MonoBehaviour
{
    [Header("Prefabs")]
    [Tooltip("Lista de 7 prefabs de gate na ordem AND=0, NAND=1, OR=2, NOR=3, NOT=4, XOR=5, XNOR=6")]
    public List<GameObject> gatePrefabs;
    public GameObject inputNodePrefab;
    public GameObject staticNodePrefab;
    public Transform circuitParent;

    [Header("Dificuldade")]
    public DifficultyConfig difficultyConfig;
    [Range(0, 1)]
    public int desiredOutput = 1;

    [Header("Layout (X/Z)")]
    public float unitSize = 1f;
    public float spacing = 0.5f;

    [Header("Inputs do Jogador")]
    [Min(1)]
    [Tooltip("Quantos nós de entrada o jogador terá")]
    public int initialInputCount = 4;


    // --- Modelo interno ---
    private GateNode outputRoot;
    private readonly List<GateNode> inputLeaves = new();
    private readonly Dictionary<GateNode, GameObject> nodeToGO = new();
    private StaticValueNodeView externalStatic;

    void Start()
    {
        if (!ValidateSetup()) return;

        // Limpa circuito anterior
        foreach (Transform ch in circuitParent) Destroy(ch.gameObject);

        // 1) Static externo
        var goExt = Instantiate(staticNodePrefab, Vector3.zero, Quaternion.identity, circuitParent);
        externalStatic = goExt.GetComponent<StaticValueNodeView>();
        externalStatic.Initialize(desiredOutput);

        // 2) Gera modelo sem nós estáticos internos
        GenerateModel();

        // 3) Valida solvência apenas com inputs
        if (!ValidateSolvable())
        {
            Debug.LogError("❌ Puzzle impossível com apenas entradas de jogador.");
            return;
        }

        // 4) Constroi a interface
        BuildView();
    }

    bool ValidateSetup()
    {
        if (gatePrefabs == null || gatePrefabs.Count != Enum.GetValues(typeof(GateNode.GateType)).Length)
        {
            Debug.LogError("⚠️ gatePrefabs deve ter 7 elementos na ordem de GateType.");
            return false;
        }
        if (inputNodePrefab == null || staticNodePrefab == null || circuitParent == null || difficultyConfig == null)
        {
            Debug.LogError("⚠️ Atribua inputNodePrefab, staticNodePrefab, circuitParent e difficultyConfig.");
            return false;
        }
        return true;
    }

    void GenerateModel()
    {
        inputLeaves.Clear();
        int D = difficultyConfig.maxDepth;
        var layers = new List<List<GateNode>>(D + 1);
        for (int d = 0; d <= D; d++) layers.Add(new List<GateNode>());

        // 1) cria inputs
        for (int i = 0; i < initialInputCount; i++)
        {
            var leaf = new GateNode();
            layers[D].Add(leaf);
            inputLeaves.Add(leaf);
        }

        // 2) gera gates em cada layer D–1 .. 1
        for (int depth = D - 1; depth >= 1; depth--)
        {
            // número aleatório de gates nesta camada
            int minG = difficultyConfig.minGatesPerLayer;
            int maxG = difficultyConfig.maxGatesPerLayer;
            int gateCount = UnityEngine.Random.Range(minG, maxG + 1);

            // curvas por depth
            float fanOutChance = difficultyConfig.fanOutChanceByDepth.Evaluate(depth);
            float shortcutChance = difficultyConfig.shortcutChanceByDepth.Evaluate(depth);
            float loopChance = difficultyConfig.loopChanceByDepth.Evaluate(depth);

            var thisLayer = layers[depth];
            var lowerLayer = layers[depth + 1];
            var evenLower = depth + 2 <= D ? layers[depth + 2] : null;

            for (int g = 0; g < gateCount; g++)
            {
                var gate = new GateNode
                {
                    Type = (GateNode.GateType)UnityEngine.Random.Range(0, gatePrefabs.Count)
                };

                // Decide quantas entradas, e de que layer vêm:
                int inMin = difficultyConfig.minInputs;
                int inMax = difficultyConfig.maxInputs;
                int numIn = gate.Type == GateNode.GateType.NOT
                          ? 1
                          : UnityEngine.Random.Range(inMin, inMax + 1);

                for (int e = 0; e < numIn; e++)
                {
                    // escolhe provider
                    GateNode provider;
                    float r = UnityEngine.Random.value;

                    if (r < fanOutChance && lowerLayer.Count > 0)
                    {
                        // reuse (fan-out)
                        provider = lowerLayer[UnityEngine.Random.Range(0, lowerLayer.Count)];
                    }
                    else if (r < fanOutChance + shortcutChance && evenLower != null && evenLower.Count > 0)
                    {
                        // atalho across two layers
                        provider = evenLower[UnityEngine.Random.Range(0, evenLower.Count)];
                    }
                    else
                    {
                        // ligação “normal” à camada imediatamente abaixo
                        provider = lowerLayer[UnityEngine.Random.Range(0, lowerLayer.Count)];
                    }

                    // Link
                    provider.Outputs.Add(gate);
                    gate.Inputs.Add(provider);
                }

                thisLayer.Add(gate);
            }

            // 3) loops: chance de criar um ciclo entre portas desta camada e da superior
            if (difficultyConfig.allowLoops && UnityEngine.Random.value < loopChance)
            {
                var from = thisLayer[UnityEngine.Random.Range(0, thisLayer.Count)];
                var toDepth = UnityEngine.Random.Range(1, depth);
                var toList = layers[toDepth];
                if (toList.Count > 0)
                {
                    var to = toList[UnityEngine.Random.Range(0, toList.Count)];
                    // evita ciclos grandes demais
                    if (ComputePathLength(to, from) < difficultyConfig.maxLoopLength)
                    {
                        from.Outputs.Add(to);
                        to.Inputs.Add(from);
                    }
                }
            }
        }

        // 4) escolhe o root de layer 1
        var topLayer = layers[1];
        outputRoot = topLayer[UnityEngine.Random.Range(0, topLayer.Count)];
    }

    /// <summary>
    /// Retorna o menor caminho (BFS) de 'start' até 'end', ou grande se não houver.
    /// </summary>
    int ComputePathLength(GateNode start, GateNode end)
    {
        var q = new Queue<(GateNode, int)>();
        var seen = new HashSet<GateNode> { start };
        q.Enqueue((start, 0));
        while (q.Count > 0)
        {
            var (cur, dist) = q.Dequeue();
            if (cur == end) return dist;
            foreach (var nxt in cur.Outputs)
                if (!seen.Contains(nxt))
                {
                    seen.Add(nxt);
                    q.Enqueue((nxt, dist + 1));
                }
        }
        return int.MaxValue;
    }



    void BuildSubTree(GateNode node, int depth)
    {
        int cnt = node.Type == GateNode.GateType.NOT
                ? 1
                : URandom.Range(difficultyConfig.minInputs, difficultyConfig.maxInputs + 1);

        for (int i = 0; i < cnt; i++)
        {
            // Se chegou na profundidade máxima, vira folha de input (possível fan-out)
            if (depth >= difficultyConfig.maxDepth)
            {
                float fanChance = difficultyConfig.fanOutChanceByDepth.Evaluate(depth);
                if (inputLeaves.Count > 0 && URandom.value < fanChance)
                {
                    // Reusa aleatoriamente um InputNode já existente
                    var reuse = inputLeaves[URandom.Range(0, inputLeaves.Count)];
                    Link(reuse, node);
                }
                else
                {
                    // Cria um novo InputNode
                    var leaf = new GateNode();
                    Link(leaf, node);
                    inputLeaves.Add(leaf);
                }
            }
            else
            {
                // Continua criando gates intermediários
                var child = new GateNode
                {
                    Type = (GateNode.GateType)URandom.Range(0, gatePrefabs.Count)
                };
                Link(child, node);
                BuildSubTree(child, depth + 1);
            }
        }
    }


    void Link(GateNode from, GateNode to)
    {
        from.Outputs.Add(to);
        to.Inputs.Add(from);
    }

    bool ValidateSolvable()
    {
        int n = inputLeaves.Count;
        if (n > 20) return true;  // evita explosão

        int combos = 1 << n;
        for (int mask = 0; mask < combos; mask++)
        {
            // marca cada leaf como se fosse input do jogador
            for (int i = 0; i < n; i++)
            {
                var leaf = inputLeaves[i];
                leaf.IsStaticValue = true;
                leaf.StaticValue = (mask >> i) & 1;
            }

            if (outputRoot.Evaluate() == desiredOutput)
            {
                // reset
                foreach (var leaf in inputLeaves) leaf.IsStaticValue = false;
                return true;
            }
        }

        foreach (var leaf in inputLeaves) leaf.IsStaticValue = false;
        return false;
    }

    void BuildView()
    {
        nodeToGO.Clear();
        float step = unitSize + spacing;

        // Calcula depth (root = 1)
        var depthMap = new Dictionary<GateNode, int>();
        ComputeDepth(outputRoot, 1, depthMap);

        // Agrupa e instancia
        var layers = depthMap
            .GroupBy(kv => kv.Value)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Key).ToList());

        foreach (var kv in layers)
        {
            int depth = kv.Key;
            var nodes = kv.Value;
            float zSpan = (nodes.Count - 1) * step;

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                GameObject go;

                if (inputLeaves.Contains(node))
                    go = Instantiate(inputNodePrefab, circuitParent);
                else
                    go = Instantiate(gatePrefabs[(int)node.Type], circuitParent);

                // posiciona em X=depth*step, Z espalhado
                float x = depth * step;
                float z = -zSpan / 2 + i * step;
                go.transform.localPosition = new Vector3(x, 0, z);

                if (go.TryGetComponent<GateView>(out var gv))
                {
                    gv.SetModel(node);
                    gv.InitializeView(node.Type, node.Inputs.Count, OnAnyInputChanged);
                    gv.UpdateOutputHint(null);
                }
                else if (go.TryGetComponent<InputNodeView>(out var iv))
                {
                    iv.SetModel(node);
                    iv.Initialize(OnAnyInputChanged);
                }

                nodeToGO[node] = go;
            }
        }
    }

    void ComputeDepth(GateNode n, int d, Dictionary<GateNode, int> map)
    {
        if (map.ContainsKey(n)) return;
        map[n] = d;
        foreach (var c in n.Inputs) ComputeDepth(c, d + 1, map);
    }

    private bool NodeReady(GateNode node)
    {
        if (node.IsStaticValue) return true;
        return node.Inputs.All(inp => NodeReady(inp));
    }

    void OnAnyInputChanged()
    {
        // Atualiza todos os gates
        foreach (var kv in nodeToGO)
        {
            var node = kv.Key;
            var go = kv.Value;
            if (inputLeaves.Contains(node)) continue;

            var gv = go.GetComponent<GateView>();
            bool ready = NodeReady(node);
            gv.UpdateOutputHint(ready ? gv.Evaluate() : (int?)null);
        }

        // Feedback de vitória
        if (NodeReady(outputRoot) && outputRoot.Evaluate() == desiredOutput)
        {
            Debug.Log("🎉 Puzzle resolvido!");
            externalStatic.HighlightSuccess();
        }
    }
}
