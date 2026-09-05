# Senior Full Stack Developer — Technical Take-Home

**Time budget: 3 hours.** Not 3 hours plus a weekend. We time-box deliberately and we assess what you did with the budget, not how much you built.

**AI tools are explicitly allowed** — Claude, Copilot, Cursor, ChatGPT, whatever you use daily. We use them too. What we are hiring is the engineer driving the tool, so we ask you to show us your driving, not just your destination.

---

## 1. What we are actually assessing

Read this section carefully. Everything below is designed around it.

| We want to see | How we will see it |
|---|---|
| You understood the problem, including the parts that don't quite add up | Your written assumptions and questions |
| You designed before you coded | Your design document and your commit history |
| You can model a domain, not just persist rows | Your entities, invariants and state transitions |
| It actually runs | We clone and run it |
| Authorisation works, including the non-obvious rules | We call your API as different users and try things |
| Cross-cutting concerns are handled once, not sprinkled | Your middleware/pipeline and frontend equivalents |
| You use patterns and SOLID because they solve a problem here | Your code, and your ability to defend the choices |
| You can **define, implement and enforce** a requirement | Your traceability document and your automated enforcement |
| You direct AI rather than accept its output | Your AI usage note, and the follow-up interview |

There is a **45–60 minute follow-up interview** where you will walk us through your architecture and make a small change to your own code, live. Build something you can explain.

---

## 2. The domain

You are building a slice of a **construction cost estimating** tool.

A **Project** has one or more **Estimates**. An Estimate is a list of **Estimate Lines**. Each line references an item from a **Rate Catalogue** and records a quantity; the line's cost is derived from the catalogue rate, the quantity and a markup.

Estimates move through a lifecycle: an estimator builds one up in **Draft**, **submits** it for review, and a reviewer **approves** or **rejects** it. Approved estimates are the commercial record for the project.

### 2.1 Business rules

**Rate catalogue**

- R1. A catalogue item has a code (e.g. `CONC-C30`), a description, a unit of measure (e.g. `m3`, `hr`, `each`), a **rate** and a **labour cost component** of that rate.
- R2. Rates are **effective-dated**. `CONC-C30` may be 1 450.00 from 1 Jan and 1 512.50 from 1 Jul. An estimate line must use the rate effective on the **estimate's pricing date**, not today's date.

**Estimate lines**

- R3. A line records a quantity (up to 3 decimal places) and a markup percentage (up to 2 decimal places, 0–100).
- R4. Line net = quantity × rate. Line total = net + markup. **All monetary values presented or stored as money are accurate to 2 decimal places.**
- R5. The estimate total is the sum of its lines.
- R6. A line's quantity must be greater than zero.
- R7. A catalogue item may appear at most once per estimate. Adding the same item again adjusts the existing line.

**Lifecycle**

- R8. Draft → Submitted → Approved, or Submitted → Rejected → Draft. No other transitions.
- R9. An estimate may only be submitted if it has at least one line and its pricing date is not in the future.
- R10. **An approved estimate cannot be changed.**
- R11. A project manager can correct a typo in an approved estimate's description at any time.
- R12. Approving an estimate must be safe to retry — a duplicate approval request must not produce a second approval or a second audit entry.
- R13. Every lifecycle transition is recorded with who did it and when.

**People and permissions**

> **You are not building authentication.** There is no login, no password handling and no token issuing in this exercise — we have taken it out deliberately so you can spend the time elsewhere. Identity arrives on every request in an `X-User-Email` header, and the starter repo has a ten-line handler that reads it. Everything below still applies.

- R14. Three roles: **Estimator**, **Reviewer**, **Viewer**.
- R15. Estimators create and edit draft estimates and submit them. Reviewers approve or reject submitted estimates. Viewers read only.
- R16. A reviewer may not approve an estimate they created.
- R17. **Estimators must not be able to see the labour cost component of any rate.** Reviewers and project managers may.
- R18. Users only see projects they are assigned to.
- R19. The `X-User-Email` header carries **identity only**. A user's role and project assignments are yours to resolve from your own data — a client that asserts its own role must not be believed.

**Scale**

- R20. A real project carries up to 50 000 estimate lines. The API must return an estimate and its lines to the UI.

---

## 3. What to build

**Backend: .NET Web API. This is fixed.**

**Everything else is your call** — React or Angular on the front, and PostgreSQL, SQL Server or a NoSQL store for persistence. We want the choice, and we want the reasoning behind it written down.

### 3.1 Minimum functional slice

Three hours is not enough to build all of section 2. It is not meant to be. Build this slice well:

1. **API** — enough endpoints to: list my projects, open one estimate with its lines, add/update a line, and submit and approve an estimate.
2. **Authorisation** — the rules in R14–R19, enforced server-side. Identity is handed to you; deciding what that identity is allowed to see and do is the part we are assessing.
3. **Frontend** — one screen is plenty: an estimate view where a user can see lines and totals, edit a line, and act on the estimate according to their role. Put a role switcher somewhere so we can change user without restarting anything.
4. **Persistence** — real, in your chosen store, with the seed data provided.
5. **Tests** — a small number of tests that prove the business rules you consider highest-risk. Not coverage. Evidence.

We removed authentication from this exercise because it is the most thoroughly solved, most easily generated part of any codebase, and it told us nothing. **The time it would have taken is not a bonus — it is the budget for doing the rest properly.**

### 3.2 We would rather see

Three endpoints that are correct, secured, validated, logged and tested than twelve that are none of those things. **Cut scope loudly and tell us what you cut and why.** Deliberately dropping R18 with a one-line justification scores better than a broken half-implementation of it.

---

## 4. Deliverables

A Git repository (public repo, or a zipped repo **with the `.git` folder intact** — the history is part of the submission), containing your code and the following documents.

### 4.1 `docs/DESIGN.md` — write this first, commit it before you write code

We look at the timestamps. Roughly 30 minutes of your budget belongs here.

- **The problem in your own words.** Two or three paragraphs. Not a restatement of this brief.
- **Assumptions, ambiguities and questions.** Section 2 is written the way real requirements arrive: some of it is under-specified and at least one part of it does not hold together. Tell us what you found, what you decided, and what you would have asked us. *This section carries real weight in our scoring.*
- **Architecture.** A diagram (Mermaid, ASCII, a photo of paper — we do not care) and a short walkthrough: layers or slices, what depends on what, where the boundary rules are.
- **Domain model.** Entities, value objects, aggregate boundaries, invariants, and the state machine.
- **Scope.** What you are building in the three hours, what you are deliberately not building, and what you would do next with another day.

### 4.2 `docs/ADR/` — two or three architecture decision records

Short. One page each, maximum. Use the template in the starter repo. At minimum, one for your frontend framework choice and one for your persistence choice. For each: the decision, the alternatives you rejected, and what would make you change your mind.

### 4.3 `docs/REQUIREMENTS.md` — define, implement, enforce

A table. One row per rule from section 2 that you implemented:

| Rule | How it is defined | Where it is implemented | How it is enforced |
|---|---|---|---|

"Enforced" means something that fails automatically when the rule is broken — a test, an analyzer, an architecture test, a database constraint, a CI gate, a type that makes the invalid state unrepresentable. A code comment is not enforcement.

**At least one of your enforcement mechanisms must be structural rather than a unit test** — for example an architecture test asserting your domain layer has no dependency on EF Core or ASP.NET, an analyzer or lint rule, or a CI check. We will try to break it during the interview and watch it fail.

Rules you chose not to implement belong in the table too, marked as such, with a reason.

### 4.4 `docs/AI-USE.md`

Genuinely no penalty for heavy AI use, and no bonus for avoiding it. We want:

- Which tools you used and for what.
- **One concrete example of AI output you rejected or rewrote, and why.** Paste the before and after.
- Anything in your submission you would not be comfortable defending line by line. Naming it here costs you nothing. Not naming it, and then being unable to explain it in the interview, costs you a lot.

### 4.5 `README.md`

How to run it, ideally in one command. If we cannot start your app in ten minutes on a clean machine, we assess what we can read and nothing more. Tell us how to act as each role — which header value, or which control in the UI.

### 4.6 Commit history

Commit as you work — we want to see the shape of the three hours, not one `initial commit` at the end. Design document first.

---

## 5. Suggested time budget

| | |
|---|---|
| Read, question, design, write `DESIGN.md` | 30 min |
| Scaffold and wire up | 20 min |
| Backend: domain, API, authorisation, cross-cutting | 65 min |
| Frontend | 35 min |
| Tests, enforcement, `REQUIREMENTS.md`, `AI-USE.md`, README | 30 min |
| **Total** | **180 min** |

Going over 3 hours is not a disqualification, but tell us in the README how long it actually took. An honest "this took me 4h15" is fine. We will find out in the interview anyway.

---

## 6. The follow-up interview

45–60 minutes, and it is the other half of this assessment.

- You walk us through your architecture **without your code on screen** for the first ten minutes.
- We ask why, on your own decisions and on the alternatives you rejected.
- You make a **small extension to your own code, live, with us watching**. You may use AI. We are interested in how you brief it, what you check, and what you do when it is wrong.
- We ask what breaks first at 50 000 lines.

If someone else's code shows up in that room, the interview will find it in about four minutes. That is not a threat, it is just how it goes — and it is why we would rather you submit a smaller thing you own completely.

---

## 7. Starter repository

A scaffold is provided: solution and folder structure, `docker-compose.yml` for PostgreSQL and SQL Server, seed data (users, projects, rate catalogue with effective-dated rates), a ready-to-paste development identity handler for the `X-User-Email` header, document templates, and a CI workflow stub. It contains **no domain code and no authorisation logic** — the modelling and the permission rules are the assessment.

You are free to ignore it entirely and start from `dotnet new`. Using it just saves you setup minutes.

---

## 8. Ground rules

- Use any library, framework, template or generator you like. Attribute anything substantial you did not write.
- Do not commit real secrets. A `.env.example` with development values is expected.
- If something in section 2 is unclear, **decide, document the decision, and move on**. Do not email us and wait — how you handle ambiguity without access to the customer is part of what we are measuring.
- Submit by the agreed date with a link or a zip.

Good luck. We are looking forward to the conversation.
