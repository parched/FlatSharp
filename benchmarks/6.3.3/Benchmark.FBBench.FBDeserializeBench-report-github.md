``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 11 (10.0.22621.819)
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK=7.0.100
  [Host]   : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  ShortRun : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2

Job=ShortRun  AnalyzeLaunchVariance=True  Runtime=.NET 7.0  
IterationCount=5  LaunchCount=7  WarmupCount=3  

```
|                                         Method | TraversalCount | DeserializeOption | VectorLength |     Mean |     Error |    StdDev |      P25 |      P95 |   Gen0 |   Gen1 | Allocated |
|----------------------------------------------- |--------------- |------------------ |------------- |---------:|----------:|----------:|---------:|---------:|-------:|-------:|----------:|
|                     **FlatSharp_ParseAndTraverse** |              **1** |              **Lazy** |           **30** | **1.891 μs** | **0.0229 μs** | **0.0370 μs** | **1.865 μs** | **1.950 μs** | **0.4864** |      **-** |   **7.95 KB** |
|              FlatSharp_ParseAndTraversePartial |              1 |              Lazy |           30 | 1.471 μs | 0.0053 μs | 0.0083 μs | 1.466 μs | 1.482 μs | 0.4864 |      - |   7.95 KB |
|        FlatSharp_ParseAndTraverse_ValueStructs |              1 |              Lazy |           30 | 1.876 μs | 0.0127 μs | 0.0201 μs | 1.860 μs | 1.917 μs | 0.3719 |      - |   6.08 KB |
| FlatSharp_ParseAndTraversePartial_ValueStructs |              1 |              Lazy |           30 | 1.625 μs | 0.0037 μs | 0.0058 μs | 1.621 μs | 1.635 μs | 0.3719 |      - |   6.08 KB |
|                     **FlatSharp_ParseAndTraverse** |              **1** |       **Progressive** |           **30** | **2.437 μs** | **0.0374 μs** | **0.0593 μs** | **2.381 μs** | **2.506 μs** | **0.6218** | **0.0191** |  **10.19 KB** |
|              FlatSharp_ParseAndTraversePartial |              1 |       Progressive |           30 | 1.881 μs | 0.0200 μs | 0.0324 μs | 1.861 μs | 1.962 μs | 0.6218 | 0.0210 |  10.19 KB |
|        FlatSharp_ParseAndTraverse_ValueStructs |              1 |       Progressive |           30 | 2.310 μs | 0.0172 μs | 0.0268 μs | 2.283 μs | 2.354 μs | 0.4921 | 0.0114 |   8.08 KB |
| FlatSharp_ParseAndTraversePartial_ValueStructs |              1 |       Progressive |           30 | 1.998 μs | 0.0103 μs | 0.0166 μs | 1.987 μs | 2.022 μs | 0.4921 | 0.0114 |   8.08 KB |
|                     **FlatSharp_ParseAndTraverse** |              **1** |            **Greedy** |           **30** | **2.011 μs** | **0.0167 μs** | **0.0269 μs** | **1.995 μs** | **2.055 μs** | **0.5188** | **0.0153** |   **8.49 KB** |
|              FlatSharp_ParseAndTraversePartial |              1 |            Greedy |           30 | 1.851 μs | 0.0206 μs | 0.0339 μs | 1.825 μs | 1.917 μs | 0.5188 | 0.0153 |   8.49 KB |
|        FlatSharp_ParseAndTraverse_ValueStructs |              1 |            Greedy |           30 | 2.035 μs | 0.0126 μs | 0.0192 μs | 2.020 μs | 2.066 μs | 0.4616 | 0.0114 |   7.55 KB |
| FlatSharp_ParseAndTraversePartial_ValueStructs |              1 |            Greedy |           30 | 1.916 μs | 0.0140 μs | 0.0223 μs | 1.904 μs | 1.967 μs | 0.4616 | 0.0114 |   7.55 KB |
|                     **FlatSharp_ParseAndTraverse** |              **1** |     **GreedyMutable** |           **30** | **1.973 μs** | **0.0142 μs** | **0.0229 μs** | **1.951 μs** | **2.009 μs** | **0.5150** | **0.0153** |   **8.47 KB** |
|              FlatSharp_ParseAndTraversePartial |              1 |     GreedyMutable |           30 | 1.838 μs | 0.0208 μs | 0.0341 μs | 1.803 μs | 1.881 μs | 0.5169 | 0.0153 |   8.47 KB |
|        FlatSharp_ParseAndTraverse_ValueStructs |              1 |     GreedyMutable |           30 | 2.008 μs | 0.0112 μs | 0.0171 μs | 1.995 μs | 2.041 μs | 0.4578 | 0.0114 |   7.53 KB |
| FlatSharp_ParseAndTraversePartial_ValueStructs |              1 |     GreedyMutable |           30 | 1.876 μs | 0.0099 μs | 0.0154 μs | 1.866 μs | 1.901 μs | 0.4597 | 0.0114 |   7.53 KB |
|                     **FlatSharp_ParseAndTraverse** |              **5** |              **Lazy** |           **30** | **8.956 μs** | **0.0993 μs** | **0.1574 μs** | **8.864 μs** | **9.216 μs** | **2.4109** |      **-** |  **39.42 KB** |
|              FlatSharp_ParseAndTraversePartial |              5 |              Lazy |           30 | 7.099 μs | 0.0651 μs | 0.1052 μs | 7.045 μs | 7.263 μs | 2.4109 |      - |  39.42 KB |
|        FlatSharp_ParseAndTraverse_ValueStructs |              5 |              Lazy |           30 | 9.179 μs | 0.1292 μs | 0.2086 μs | 9.079 μs | 9.683 μs | 1.8311 |      - |  30.05 KB |
| FlatSharp_ParseAndTraversePartial_ValueStructs |              5 |              Lazy |           30 | 7.901 μs | 0.0429 μs | 0.0692 μs | 7.846 μs | 8.023 μs | 1.8311 |      - |  30.05 KB |
|                     **FlatSharp_ParseAndTraverse** |              **5** |       **Progressive** |           **30** | **4.581 μs** | **0.0637 μs** | **0.1028 μs** | **4.492 μs** | **4.744 μs** | **0.6180** | **0.0153** |  **10.19 KB** |
|              FlatSharp_ParseAndTraversePartial |              5 |       Progressive |           30 | 3.148 μs | 0.0321 μs | 0.0491 μs | 3.126 μs | 3.239 μs | 0.6218 | 0.0191 |  10.19 KB |
|        FlatSharp_ParseAndTraverse_ValueStructs |              5 |       Progressive |           30 | 4.145 μs | 0.0153 μs | 0.0246 μs | 4.126 μs | 4.179 μs | 0.4883 | 0.0076 |   8.08 KB |
| FlatSharp_ParseAndTraversePartial_ValueStructs |              5 |       Progressive |           30 | 3.300 μs | 0.0097 μs | 0.0154 μs | 3.290 μs | 3.326 μs | 0.4921 | 0.0114 |   8.08 KB |
|                     **FlatSharp_ParseAndTraverse** |              **5** |            **Greedy** |           **30** | **3.714 μs** | **0.0477 μs** | **0.0728 μs** | **3.644 μs** | **3.836 μs** | **0.5188** | **0.0153** |   **8.49 KB** |
|              FlatSharp_ParseAndTraversePartial |              5 |            Greedy |           30 | 2.838 μs | 0.0518 μs | 0.0852 μs | 2.774 μs | 2.956 μs | 0.5188 | 0.0153 |   8.49 KB |
|        FlatSharp_ParseAndTraverse_ValueStructs |              5 |            Greedy |           30 | 3.747 μs | 0.0184 μs | 0.0287 μs | 3.725 μs | 3.788 μs | 0.4616 | 0.0114 |   7.55 KB |
| FlatSharp_ParseAndTraversePartial_ValueStructs |              5 |            Greedy |           30 | 3.158 μs | 0.0196 μs | 0.0310 μs | 3.138 μs | 3.232 μs | 0.4616 | 0.0114 |   7.55 KB |
|                     **FlatSharp_ParseAndTraverse** |              **5** |     **GreedyMutable** |           **30** | **3.538 μs** | **0.0352 μs** | **0.0569 μs** | **3.498 μs** | **3.634 μs** | **0.5150** | **0.0153** |   **8.47 KB** |
|              FlatSharp_ParseAndTraversePartial |              5 |     GreedyMutable |           30 | 2.731 μs | 0.0357 μs | 0.0556 μs | 2.703 μs | 2.820 μs | 0.5150 | 0.0153 |   8.47 KB |
|        FlatSharp_ParseAndTraverse_ValueStructs |              5 |     GreedyMutable |           30 | 3.644 μs | 0.0104 μs | 0.0165 μs | 3.633 μs | 3.675 μs | 0.4578 | 0.0114 |   7.53 KB |
| FlatSharp_ParseAndTraversePartial_ValueStructs |              5 |     GreedyMutable |           30 | 3.030 μs | 0.0192 μs | 0.0304 μs | 3.008 μs | 3.098 μs | 0.4578 | 0.0114 |   7.53 KB |