# Spec: Agent Folder Hidden-to-Normal Migration Design

## 1. 개요
IDE 및 파일 에디터가 숨김 폴더(`.agent/`) 내의 마크다운 파일을 로드하지 못하는 오류를 극복하기 위해, 해당 에이전트 폴더를 일반 형태의 디렉토리인 `Agent/`로 통째로 마이그레이션(이동)합니다.

## 2. 요구사항 및 목표
- Git 추적 히스토리를 보존하면서 `/.agent/` 폴더를 `/Agent/` 폴더로 변경합니다.
- `rules/` 내의 규칙 파일 3개, `docs/` 내의 아키텍처 가이드 1개 및 사전 과제 PDF 문서를 포함한 모든 내부 파일을 온전히 이동시킵니다.
- 숨김 속성이 해제됨으로써 Rider, VS Code 등 모든 Unity 개발 환경에서 에이전트 규칙서를 즉각적으로 확인하고 읽을 수 있도록 만듭니다.

## 3. 마이그레이션 대상 경로 명세
- 이전: `/.agent/`
- 변경: `/Agent/`

구체적인 매핑 정보:
- `/.agent/rules/architecture-rules.md` → `/Agent/rules/architecture-rules.md`
- `/.agent/rules/event-system-rules.md` → `/Agent/rules/event-system-rules.md`
- `/.agent/rules/safety-rules.md` → `/Agent/rules/safety-rules.md`
- `/.agent/docs/architecture-guide.md` → `/Agent/docs/architecture-guide.md`
- `/.agent/docs/BePex Unity Client Programmer 사전 과제 윤승종.pdf` → `/Agent/docs/BePex Unity Client Programmer 사전 과제 윤승종.pdf`

## 4. 검증 계획
- `/.agent/` 폴더가 더 이상 리포지토리에 남아있지 않는지 확인합니다.
- `/Agent/` 및 그 하위 파일들이 정상 경로에 위치하고 내용을 정상적으로 읽을 수 있는지 확인합니다.
- `git status` 상에 `renamed:` 이력으로 안정적으로 추적되는지 검증합니다.
