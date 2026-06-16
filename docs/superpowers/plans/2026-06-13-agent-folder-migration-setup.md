# Agent Folder Hidden-to-Normal Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 개발 환경에서의 정상적인 로딩과 뷰 연동을 보장하기 위해 숨김 폴더인 `.agent/`를 일반 폴더인 `Agent/`로 전체 마이그레이션(이동)합니다.

**Architecture:** Git 추적 안정성을 유지하면서 디렉토리를 이전하도록 `git mv` 명령어를 활용하여 통째로 이동시키고, 이전 폴더 삭제 유무와 신규 폴더 내 파일 존재 여부를 검증합니다.

**Tech Stack:** Git (버전 관리)

---

### Task 1: 디렉토리 마이그레이션 및 커밋

**Files:**
- Modify: `/.agent/` -> `/Agent/` (전체 이동)

- [ ] **Step 1: Execute git mv command**

  프로젝트 루트에서 디렉토리 통째로 이동하는 git mv 명령을 수행합니다.
  Run:
  ```bash
  git mv .agent Agent
  ```

- [ ] **Step 2: Verify folder paths**

  1. 이전 `.agent` 폴더가 더 이상 존재하지 않는지 검증:
     `test ! -d /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/.agent`
     Expected: 성공(존재하지 않음)
  2. 신규 `Agent` 폴더와 하위 파일이 존재하는지 검증:
     `test -f /Users/woodenshield/Desktop/UNITY/Project/BePex_Unity_Client/Agent/rules/event-system-rules.md`
     Expected: 성공(존재함)

- [ ] **Step 3: Commit migration changes**

  이동 내역을 커밋합니다.
  Run:
  ```bash
  git commit -m "refactor: migrate agent folder from hidden (.agent) to normal (Agent) directory"
  ```
