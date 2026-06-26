# SimpleMusic 测试报告

## 测试框架

xUnit

## 测试覆盖

| 模块     | 测试类            | 用例数 |
| -------- | ----------------- | ------ |
| 歌词解析 | LyricParserTests  | 4      |
| 歌曲扫描 | MusicScannerTests | 1      |

## 测试用例

### LyricParserTests

| 用例                   | 输入                         | 预期      | 结果 |
| ---------------------- | ---------------------------- | --------- | ---- |
| Parse_Standard         | `[00:01.50]测试`             | Time=1.5s | 通过 |
| Parse_ThreeDigitMs     | `[00:01.500]测试`            | Time=1.5s | 通过 |
| Parse_NoMs             | `[00:01]测试`                | Time=1s   | 通过 |
| FindCurrentLine_Middle | lines=[1s,3s,5s], current=2s | 索引0     | 通过 |

### MusicScannerTests

| 用例             | 输入     | 预期   | 结果 |
| ---------------- | -------- | ------ | ---- |
| ScanFolder_Empty | 空文件夹 | 空列表 | 通过 |

## TDD实践

1. **Red**：先写测试定义接口契约
2. **Green**：实现功能使测试通过
3. **Refactor**：优化代码结构

LyricParser通过TDD确保了解析逻辑覆盖各种LRC格式变体，避免了后期Bug修复成本。

## 已知问题

- 进度条拖动时存在轻微抖动（不影响核心功能）
