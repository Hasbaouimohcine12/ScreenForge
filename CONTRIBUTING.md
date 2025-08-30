# Contribuindo

Obrigado por colaborar com o ScreenForge!

## Requisitos
- .NET 8 SDK
- FFmpeg local para dev (não commitamos binários; veja README)

## Fluxo
1. Crie um branch a partir de `main`: `git checkout -b feat/nome-curto`
2. Faça commits com **Conventional Commits**:
   - `feat:`, `fix:`, `docs:`, `chore:`, `refactor:`, `test:`, etc.
   - Use `npm run commit` para ajudar.
3. Rode `dotnet build` e (se houver) `dotnet test`.
4. Abra o PR. Descreva motivação e screenshots/gifs.

## Commits
- Padrão: `<type>(escopo opcional): descrição`
- Ex.: `feat(recorder): opção de gravar apenas monitor 2`

## PR
- Uma mudança por PR.
- Marque como `draft` se ainda estiver WIP.

## Código
- Siga `.editorconfig`.
- Evite warnings novos.
