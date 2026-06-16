# Spec: Permanent Migration of Agent Folder to Avoid Editor Read Failure

## 1. 개요
숨김 폴더(`.agent/`) 하위 파일 접근 시 일부 에디터 및 감시 시스템이 존재하지 않는 파일로 차단하는 로딩 오류를 완벽하게 방지하기 위해, 폴더를 마침표가 없는 일반 형태인 `Agent/`로 영구 마이그레이션(이동)합니다.

## 2. 요구사항 및 목표
- Git 히스토리를 유지하면서 `/.agent/` 폴더를 `/Agent/` 폴더로 변경합니다.
- `rules/` 및 `docs/` 하위의 모든 마크다운 규칙 파일 및 PDF 문서를 이동시킵니다.
- 숨김 속성이 해제됨으로써 에디터가 예외 없이 모든 규칙 문서를 즉각적이고 안정적으로 인식할 수 있게 합니다.

## 3. 마이그레이션 명세
- 이전: `/.agent/`
- 변경: `/Agent/`

매핑 세부사항:
- `/.agent/rules/architecture-rules.md` → `/Agent/rules/architecture-rules.md`
- `/.agent/rules/event-system-rules.md` → `/Agent/rules/event-system-rules.md`
- `/.agent/rules/safety-rules.md` → `/Agent/rules/safety-rules.md`
- `/.agent/docs/architecture-guide.md` → `/Agent/docs/architecture-guide.md`
- `/.agent/docs/BePex Unity Client Programmer 사전 과제 윤승종.pdf` → `/Agent/docs/BePex Unity Client Programmer 사전 과제 윤승종.pdf`

## 4. 검증 계획
- `/.agent/` 폴더가 더 이상 존재하지 않는지 검사합니다.
- `/Agent/rules/event-system-rules.md` 파일이 정상 존재하고 에디터가 정상적으로 읽어들이는지 수동 검증합니다.
