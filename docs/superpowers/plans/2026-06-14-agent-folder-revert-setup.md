# Agent Folder Hidden-to-Normal Revert Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 일반 폴더인 `Agent/`를 다시 마침표(`.`) 속성이 있는 숨김 폴더 `/.agent/`로 환원하여 루트 디렉토리 레이아웃을 다시 정리합니다.

**Architecture:** Git 추적 상태를 유지하면서 안전하게 이동하도록 `git mv` 명령을 사용하여 복구하며, 복구 후 이전 폴더 삭제 여부와 신규 숨김 폴더 내 파일 존재 여부를 검증합니다.

**Tech Stack:** Git (버전 관리)

---

### Task 1: 디렉토리 환원 및 커밋

**Files:**
- Modify: `/Agent/` -> `/.agent/` (전체 이동)

- [ ] **Step 1: Execute git mv command to revert**

  프로젝트 루트에서 디렉토리를 복원하는 git mv 명령을 수행합니다.
  Run:
  ```bash
  git mv Agent .agent
  ```

- [ ] **Step 2: Verify folder paths**

  1. 이전 `Agent` 폴더가 더 이상 존재하지 않는지 검증:
     `test ! -d /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Agent`
     Expected: 성공(존재하지 않음)
  2. 신규 복원된 `.agent` 폴더와 하위 파일이 존재하는지 검증:
     `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent/rules/event-system-rules.md`
     Expected: 성공(존재함)

- [ ] **Step 3: Commit revert changes**

  복원 내역을 커밋합니다.
  Run:
  ```bash
  git commit -m "refactor: revert agent folder from normal (Agent) back to hidden (.agent) directory"
  ```
