---
Title: BGud Operator Sign-In and Warehouse Session
Code: BGUD-OPERATOR-SIGNIN-001
Artifact: FEATURE
Version: 1.0
LastUpdated: 2026-09-23
---

# 1. Purpose

This feature defines how a warehouse operator identifies themselves to BGud (the BTR
warehouse mobile application) and establishes a working session fixed to one Gudang
(warehouse location).

It replaces BGud's previous practice of signing in with a BTR username and password plus a
separately selected Gudang. The operator now signs in with the Google account the company
has registered to them, while the business keeps a reliable record of **who** performed
each warehouse action and **which Gudang** it belonged to.

The feature covers sign-in, session establishment, sign-out, warehouse change, and the
login-time refresh of the operational data the operator needs for offline work. It does
not define barcode or return business rules — those belong to their respective domains.

# 2. Business Outcome

- A warehouse operator can start work on BGud using a Google account the company has
  registered to them, without maintaining or sharing a BTR password on the device.
- Every BGud session is unambiguously tied to one Gudang, and work performed in that
  session is attributed to the identified operator.
- Only personnel the company has registered may use BGud, and access ends when the company
  removes or deactivates that registration.
- Operational data (barcode registry and return-order reference data) is refreshed when a
  session starts, so the device can continue working offline afterwards.
- Work captured in one Gudang is never silently attributed to another Gudang, including
  records queued while the device was offline.

Out of outcome: BGud sign-in does not create a separate BGud user directory, does not grant
new roles, and does not change the authority of BTR user accounts or roles.

# 3. Participating Domains

| Domain | Responsibility within this feature |
| ------ | ---------------------------------- |
| Master Data (operator accounts) | Owns the authoritative BTR user account, its role, and the registration linking a Google account to that user. Authoritative source for who may sign in and with which role. |
| Inventory (warehouse operations) | Owns the Gudang (warehouse) concept and the operational context to which a session is fixed; also owns customer return (Retur) processing performed during a session. |
| Barcode Registry | Consumes the identified operator and Gudang context for barcode registration and lookup actions performed during the session. |

Google is an external identity provider used for sign-in only. It is not a BTR business
domain and never becomes a source of operator, role, or warehouse truth.

# 4. Trigger

An operator starts a BGud session:

- The operator opens BGud and no valid session exists on the device — the sign-in flow is
  presented.
- The operator signs in with a Google account and selects the Gudang they are working in.
- An operator who already holds a valid session on the device resumes directly into their
  work.

# 5. Preconditions

- The operator has an existing BTR user account that is active and valid.
- That BTR user account has exactly one Google email registered to it in the Main Office.
- The operator's BTR account carries the role that authorizes their BGud operations.
- The device has connectivity for sign-in and the login-time data refresh; offline
  operation applies after the refresh.
- The operator knows which Gudang they are working in.

# 6. Operational Flow

1. **Launch.** If the device holds a valid session, BGud opens into the operator's work
   (Home). Otherwise the sign-in flow is presented.
2. **Identify.** The operator selects their Google account. The system confirms that the
   Google email is registered to an existing, valid BTR user. If it is not registered, the
   operator is refused and asked to contact their administrator.
3. **Select Gudang.** The operator selects the working Gudang (Gudang Gamping, Gudang
   Concat, or Gudang Magelang).
4. **Establish session.** A session is created that associates the identified operator with
   the selected Gudang. The Gudang is fixed for the life of the session.
5. **Refresh operational data.** As part of starting the session, the device refreshes
   barcode registry data and return-order reference data for the session's Gudang.
6. **Work.** The operator proceeds with warehouse work. Barcode registrations and return
   orders created during the session are attributed to the identified operator and belong
   to the session's Gudang.
7. **Sign out.** Signing out clears the session and returns the operator to sign-in.
8. **Change Gudang.** Changing Gudang is treated as starting a new session: the operator
   ends the current session and selects another Gudang. Records already captured and
   queued under the previous Gudang keep that Gudang and are never silently moved.

# 7. Domain Orchestration

- **Master Data** supplies the registered operator identity and role at sign-in and remains
  the authority for that registration throughout the session.
- **Inventory** supplies the Gudang operational context; the session is bound to exactly
  one Gudang for its lifetime.
- **Barcode Registry** and the Retur processing within **Inventory** receive the identified
  operator and Gudang context so their actions are correctly attributed and scoped.
- The login-time data refresh is orchestrated as part of establishing the session, before
  the operator begins work.
- BGud itself holds only a local session referencing the operator and Gudang; it does not
  become an independent source of accounts, roles, or warehouse truth.

# 8. Constraints

- Only registered Google accounts may sign in; there is no unregistered or public access.
- One Google email maps to one BTR user, and one BTR user maps to at most one Google email.
- The operator's existing BTR role is authoritative; BGud introduces no separate role model.
- A session is fixed to one Gudang. Changing Gudang requires ending the current session.
- Records queued while offline retain the Gudang under which they were captured and are
  never re-homed to another Gudang.
- Main Office remains the single source of truth for operator accounts and their Google
  registrations.
- BGud sign-in must not require the operator to manage or enter a BTR password.
- Barcode and Retur business rules are owned by their domains and are referenced, not
  restated, by this feature.

# 9. Exceptions

| Exception | Expected outcome |
| --------- | ---------------- |
| Google account is not registered to any BTR user | Sign-in is refused; the operator is told to contact an administrator. |
| Registered BTR user is inactive or invalid | Sign-in is refused. |
| Registration is removed while a session is active | The operator can no longer be resolved for attributed actions; the session is treated as ended and the operator must sign in again. |
| No network during sign-in or refresh | A new session cannot be started; an existing valid session may continue subject to its state. |
| Gudang change attempted with queued offline records | Records stay bound to their original Gudang; the operator is informed they will be submitted under that original Gudang. |
| Google sign-in cancelled or fails | The operator remains signed out. |

# 10. Acceptance Criteria

- An operator with a registered, valid Google account can sign in and reach their work
  (Home).
- An operator whose Google account is not registered cannot sign in.
- An operator selects exactly one Gudang (Gamping, Concat, or Magelang), and the session
  reflects that Gudang.
- Barcode registrations and return orders created during a session are attributed to the
  identified BTR user.
- Barcode registry and return-order reference data are refreshed as part of starting a
  session.
- An operator with a valid existing session reaches their work without re-identifying.
- Sign-out returns BGud to sign-in and clears the session.
- Changing Gudang requires ending the current session, and records already queued under the
  previous Gudang keep that Gudang.
- No BTR password is required or retained for BGud sign-in.
- Sign-in creates no new BGud user account or role; BTR roles remain authoritative.

---

# Related Artifacts

| Artifact | Relevance |
| -------- | --------- |
| `docs/work/bgud-google-signin/BGUD-GOOGLE-SIGNIN-FEASIBILITY-ASSESSMENT.md` | Approved decisions OQ-001…OQ-010 that this feature is written against (anonymous BGud posture, Gudang session context, pre-registered account mapping, operator attribution, login-time synchronization, single change request). |
| `docs/issues/BGUD-GOOGLE-SIGNIN-ISSUE.md` | Originating request. |
| `docs/work/barcode-registry/BARCODE-REGISTRY-DOMAIN.md` | Barcode Registry domain knowledge and the Warehouse Officer actor. |
| `docs/work/barcode-registry/BARCODE-REGISTRY-UX-BLUEPRINT.md` | Historical login flow (pre-Google design); source for the Gudang selector and post-login refresh sequence. |
| `docs/work/barcode-registry/BARCODE-REGISTRY-ARCHITECTURE.md` | Historical login/authentication design (superseded for BGud by the decisions above). |
| `docs/foundation/PRODUCT.md`, `docs/foundation/DOMAIN.md`, `docs/foundation/LANDSCAPE.md`, `docs/foundation/WORKFLOW.md` | Product principles, ubiquitous language, area/systems ownership, and warehouse synchronization context. |
