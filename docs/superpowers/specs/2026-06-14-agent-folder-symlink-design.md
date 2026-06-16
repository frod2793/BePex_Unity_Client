# Spec: Reconciling Hidden Folder and Editor Access via Symlinks

## 1. 개요
사용자의 `.agent/` 숨김 폴더 고수 요구를 준수하면서, 동시에 IDE가 숨김 폴더 내부의 `event-system-rules.md` 등을 로드하지 못하는 오류를 해결하기 위해, 프로젝트 루트 최상위에 일반 마크다운 파일 링크(Symlink) 또는 미러(Mirror) 파일을 구성합니다.

## 2. 요구사항 및 목표
- 실제 에이전트 원본 파일들은 숨김 디렉토리인 `/.agent/rules/` 내부에 온전히 보존합니다.
- 사용자가 에디터에서 오류 없이 규칙 파일을 클릭해 읽을 수 있도록, 프로젝트 루트 최상위인 `/BePex_Unity_Client/`에 심볼릭 링크(Symlink) 또는 미러링된 복사 마크다운 파일을 생성합니다.
- 이를 통해 IDE 상에서 정상적으로 마크다운을 열람하고 읽을 수 있도록 사용성을 확보합니다.

## 3. 링크/미러 구성 사양
루트 디렉토리에 다음 파일들을 신설 및 연결합니다:
- `/BePex_Unity_Client/event-system-rules.md` (가리키는 대상: `/.agent/rules/event-system-rules.md`)
- `/BePex_Unity_Client/architecture-rules.md` (가리키는 대상: `/.agent/rules/architecture-rules.md`)
- `/BePex_Unity_Client/safety-rules.md` (가리키는 대상: `/.agent/rules/safety-rules.md`)

## 4. 검증 계획
- `/.agent/` 폴더가 원본 형태로 유지되고 있는지 검증합니다.
- 루트 최상위의 `event-system-rules.md` 파일을 통해 오류 없이 텍스트 내용이 정상 노출되는지 확인합니다.
