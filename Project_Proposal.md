# SimpleMusic - 本地音乐播放器

## 项目背景

现有音乐播放器软件功能臃肿、广告繁多，或界面简陋、体验不佳。SimpleMusic旨在提供一个轻量、美观、专注本地音乐播放的工具。

## 开发模式

本项目采用 **RDD（Readme Driven Development）** 驱动开发：先编写本文档明确需求与架构，再进入编码实现。通过文档先行确保目标清晰，避免开发过程中的需求漂移。

## 功能需求

1. 本地音乐库扫描与管理（MP3/FLAC/WAV）
2. 音频播放控制（播放/暂停/跳转/上一首/下一首）
3. 播放列表持久化存储（MySQL）
4. 专辑封面与LRC歌词显示
5. 深色主题UI

## 技术选型

| 技术   | 选型               | 理由                        |
| ------ | ------------------ | --------------------------- |
| 语言   | C# 12 / .NET 10    | 课程要求，WinForms原生支持  |
| UI     | WinForms           | 开发效率高，适合课程项目    |
| 音频   | NAudio 2.2         | 开源免费，API清晰，社区活跃 |
| 数据库 | MySQL 8.0 + Dapper | 真实数据库环境，轻量ORM     |
| 元数据 | TagLib#            | 支持ID3标签读取，维护活跃   |
| 测试   | xUnit              | .NET生态标准测试框架        |

## 系统架构

UI层 (MainForm)
├── 播放控制区
├── 歌曲列表
└── 歌词显示

业务层
├── AudioPlayer (NAudio封装)
├── MusicScanner (异步文件扫描)
└── LyricParser (LRC解析)

数据层
└── MusicDb (MySQL + Dapper)

## 开发环境

- Visual Studio 2026 Community
- Windows 11
- Git

## 仓库地址

https://github.com/moyilikeme/windowsWork.git
