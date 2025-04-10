# PlantUML Validation MCP Server

This project is an MCP server for validating PlantUML code.
![overview](./docs/overview.png)

## Tools
### ValidatePlantUml
Validates the provided PlantUML message. If valid, it returns "Ok". If invalid, it returns detailed error information, including an error description, the line where the error occurred, and other metadata.

![validatePlantUml](./docs/ValidatePlantUml.png)

## Usage

### 1. Run Docker Compose
Run the following command to start the server:

```bash
docker compose up -d
```

### 2. VSCode MCP Configuration

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
