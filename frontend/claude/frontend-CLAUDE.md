@AGENTS.md

## Claude Code notes (frontend)
The `AGENTS.md` next to this file is the single source of truth for frontend rules. The root `AGENTS.md` and root `CLAUDE.md` also apply (including: always respond to the user in Turkish; code, comments and commit messages stay in English).

Only Claude Code specific instructions belong here. General frontend rules stay in `AGENTS.md`.

- Work from the OpenAPI document and the decision documents under `claude/`; do not rely on memory of earlier chats.
- This project does not depend on third-party Claude design plugins or MCP servers. Do not add them as build or workflow requirements.
- Use Windows-compatible commands in scripts and instructions (the production server is Windows Server).
