# Spec: Agent Folder Hidden-to-Normal Revert Design

## 1. 개요
사용자의 최종 결정에 따라 일반 디렉토리인 `Agent/`를 다시 마침표(`.`)로 시작하는 숨김 속성 폴더인 `.agent/`로 환원하여, 프로젝트 루트 폴더 구성 요소를 직관적이고 깔끔하게 관리합니다.

## 2. 요구사항 및 목표
- Git 히스토리를 유지하면서 `Agent/` 폴더명을 `.agent/`로 다시 이동시킵니다.
- `rules/` 및 `docs/` 하위의 모든 마크다운 규칙서 및 사전 과제 PDF 파일의 경로를 복구합니다.

## 3. 마이그레이션 대상 경로 명세
- 이전: `/Agent/`
- 복원: `/.agent/`

구체적인 매핑 정보:
- `/Agent/rules/architecture-rules.md` → `/.agent/rules/architecture-rules.md`
- `/Agent/rules/event-system-rules.md` → `/.agent/rules/event-system-rules.md`
- `/Agent/rules/safety-rules.md` → `/.agent/rules/safety-rules.md`
- `/Agent/docs/architecture-guide.md` → `/.agent/docs/architecture-guide.md`
- `/Agent/docs/BePex Unity Client Programmer 사전 과제 윤승종.pdf` → `/.agent/docs/BePex Unity Client Programmer 사전 과제 윤승종.pdf`

## 4. 검증 계획
- `/Agent/` 폴더가 더 이상 리포지토리에 존재하지 않는지 확인합니다.
- `/.agent/` 디렉토리 하위에 모든 파일들이 정상적으로 복원되었는지 검사합니다.
- `git status` 상에서 안정적으로 `renamed:` 처리가 완료되었는지 검증합니다.
