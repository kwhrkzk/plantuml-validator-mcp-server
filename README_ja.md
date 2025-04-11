# Plantuml バリデーション MCP Server

このプロジェクトは、PlantumlのコードをバリデーションするためのMCPサーバーです。

## ツール
### ValidatePlantuml
提供されたPlantumlのメッセージを検証します。有効な場合は「Ok」を返します。無効な場合は、エラーの説明、エラーが発生した行、その他のメタデータを含む詳細なエラー情報を返します。

![validatePlantuml](./docs/ValidatePlantuml.png)


## sseでの使用方法

![overview-sse](./docs/overview-sse.png)

### 1. Docker Compose実行
以下のコマンドを実行してサーバーを起動します。

```bash
docker compose up -d
```

### 2. VSCodeのMCP設定

```json: settings.json
    "mcp": {
        "servers": {
            "my-plantuml-mcp-server": {
                "type": "sse",
                "url": "http://localhost:3000/sse"
            }
        }
    }
```
## dockerでの使用方法

![overview-docker](./docs/overview-docker.png)
### 1. 
```bash
cd plantuml-mcp-server-stdio
dotnet publish /t:PublishContainer
```

### 2. VSCodeのMCP設定

```json: settings.json
    "mcp": {
        "servers": {
            "my-plantuml-mcp-server-docker": {
                "type": "stdio",
                "command": "docker",
                "args": [
                    "run",
                    "--rm",
                    "-i",
                    "--network=host",
                    "plantuml-mcp-server-stdio",
                    "PlantumlBaseUrl=http://your_plantuml_server/"
                ],
            },
        }
    }
```