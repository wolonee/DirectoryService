<!-- BEGIN:nextjs-agent-rules -->
# This is NOT the Next.js you know

This version has breaking changes — APIs, conventions, and file structure may all differ from your training data. Read the relevant guide in `node_modules/next/dist/docs/` before writing any code. Heed deprecation notices.
<!-- END:nextjs-agent-rules -->

# AI Agent Instructions

## Commit Message Guidelines

When generating commit messages, ALWAYS follow these rules:

1. **Format**: `<type>(<scope>): <subject>`
2. **Types**: feat, fix, docs, style, refactor, test, chore
3. **Subject**: 
   - Use imperative mood ("Add" not "Added")
   - Max 50 characters
   - No period at the end
4. **Body** (if needed):
   - Explain WHY, not WHAT
   - Max 72 characters per line
5. **Footer**: Reference issues if applicable

## Examples

✅ Good: `feat(auth): add login validation`
✅ Good: `fix(api): handle null response from server`
❌ Bad: `fixed bug`
❌ Bad: `Updated files and stuff`

## Conventional Commits

We follow https://www.conventionalcommits.org/
