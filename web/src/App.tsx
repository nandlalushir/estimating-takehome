import { useEffect, useState } from "react";
import type { Estimate, Project } from "./types";

const USERS = [
  { email: "estimator@example.com", label: "Estimator" },
  { email: "reviewer@example.com", label: "Reviewer" },
  { email: "viewer@example.com", label: "Viewer" },
  { email: "manager@example.com", label: "Project Manager" }
];

const ESTIMATE_ID = "40000000-0000-0000-0000-000000000001";

function money(value: number) {
  return value.toLocaleString("en-IN", { style: "currency", currency: "INR" });
}

async function request<T>(url: string, email: string, options: RequestInit = {}) {
  const response = await fetch(url, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      "X-User-Email": email,
      ...(options.headers ?? {})
    }
  });

  if (response.status === 204) return undefined as T;

  const body = await response.json();
  if (!response.ok) throw new Error(body.detail ?? "Request failed.");
  return body as T;
}

export default function App() {
  const [email, setEmail] = useState(USERS[0].email);
  const [projects, setProjects] = useState<Project[]>([]);
  const [estimate, setEstimate] = useState<Estimate | null>(null);
  const [projectId, setProjectId] = useState("");
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");

  const load = async () => {
    setBusy(true);
    try {
      const p = await request<Project[]>("/api/projects", email);
      setProjects(p);
      const e = await request<Estimate>(`/api/estimates/${ESTIMATE_ID}`, email);
      setEstimate(e);
      setProjectId(e.projectId);
      setMessage("");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Request failed.");
      setEstimate(null);
    } finally {
      setBusy(false);
    }
  };

  useEffect(() => { void load(); }, [email]);

  async function saveLine(itemId: string, quantity: number, markupPercentage: number) {
    setBusy(true);
    try {
      await request<void>(`/api/estimates/${ESTIMATE_ID}/lines/${itemId}`, email, {
        method: "PUT",
        body: JSON.stringify({ quantity, markupPercentage })
      });
      await load();
      setMessage("Line saved.");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Could not save line.");
      setBusy(false);
    }
  }

  async function action(path: string, success: string) {
    setBusy(true);
    try {
      await request<void>(`/api/estimates/${ESTIMATE_ID}/${path}`, email, { method: "POST" });
      await load();
      setMessage(success);
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Request failed.");
      setBusy(false);
    }
  }

  async function correctDescription() {
    if (!estimate) return;
    const description = window.prompt("Correct approved description", estimate.description);
    if (!description) return;

    setBusy(true);
    try {
      await request<void>(`/api/estimates/${ESTIMATE_ID}/description`, email, {
        method: "PATCH",
        body: JSON.stringify({ description })
      });
      await load();
      setMessage("Description corrected.");
    } catch (e) {
      setMessage(e instanceof Error ? e.message : "Could not update description.");
      setBusy(false);
    }
  }

  const roleLabel = USERS.find(x => x.email === email)?.label ?? email;

  return (
    <div className="page">
      <header className="header">
        <div>
          <div className="eyebrow">CONSTRUCTION / ESTIMATING</div>
          <h1>Cost Estimating</h1>
          <p>Review and manage project estimates with server-side authorization.</p>
        </div>
        <label className="user-select">
          Acting as
          <select value={email} onChange={e => setEmail(e.target.value)}>
            {USERS.map(u => <option key={u.email} value={u.email}>{u.label} — {u.email}</option>)}
          </select>
        </label>
      </header>

      <div className="toolbar card">
        <label>
          Project
          <select value={projectId} onChange={e => setProjectId(e.target.value)}>
            {projects.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
          </select>
        </label>
        <div className="identity">
          <span className="muted">Identity:</span> {email} · <strong>{roleLabel}</strong>
        </div>
        <button onClick={() => void load()} disabled={busy}>Refresh</button>
      </div>

      {message && <div className="notice">{message}</div>}

      {estimate && (
        <>
          <section className="card estimate-head">
            <div>
              <div className="status">{estimate.status}</div>
              <h2>{estimate.description}</h2>
              <p className="muted">Pricing date: {estimate.pricingDate}</p>
            </div>
            <div className="total">
              <span>Total</span>
              <strong>{money(estimate.total)}</strong>
            </div>
          </section>

          <section className="card">
            <div className="section-title">
              <h3>Estimate lines</h3>
              <span>{estimate.lines.length} line(s)</span>
            </div>
            <div className="table-wrap">
              <table>
                <thead>
                  <tr>
                    <th>Code</th><th>Description</th><th>Qty</th><th>Rate</th>
                    <th>Markup</th><th>Labour</th><th>Total</th><th />
                  </tr>
                </thead>
                <tbody>
                  {estimate.lines.map(line => (
                    <LineRow
                      key={line.id}
                      line={line}
                      editable={estimate.status === "Draft"}
                      onSave={saveLine}
                    />
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          <section className="card actions">
            {estimate.status === "Draft" &&
              <button disabled={busy} onClick={() => void action("submit", "Estimate submitted.")}>Submit for review</button>}
            {estimate.status === "Submitted" && <>
              <button disabled={busy} onClick={() => void action("approve", "Estimate approved.")}>Approve</button>
              <button disabled={busy} onClick={() => void action("reject", "Estimate rejected.")}>Reject</button>
            </>}
            {estimate.status === "Approved" &&
              <button disabled={busy} onClick={() => void correctDescription()}>Correct approved description</button>}
          </section>
        </>
      )}

      <footer>Server authorization is authoritative; the UI only changes what actions are presented.</footer>
    </div>
  );
}

function LineRow({
  line, editable, onSave
}: {
  line: EstimateLine;
  editable: boolean;
  onSave: (id: string, quantity: number, markup: number) => Promise<void>;
}) {
  const [quantity, setQuantity] = useState(String(line.quantity));
  const [markup, setMarkup] = useState(String(line.markupPercentage));

  return (
    <tr>
      <td><strong>{line.code}</strong></td>
      <td>{line.description}<div className="muted">{line.unitOfMeasure}</div></td>
      <td><input disabled={!editable} value={quantity} onChange={e => setQuantity(e.target.value)} /></td>
      <td>{money(line.rate)}</td>
      <td><input disabled={!editable} value={markup} onChange={e => setMarkup(e.target.value)} />%</td>
      <td>{line.labourCost == null ? "—" : money(line.labourCost)}</td>
      <td><strong>{money(line.totalAmount)}</strong></td>
      <td>{editable && <button onClick={() => void onSave(line.catalogueItemId, Number(quantity), Number(markup))}>Save</button>}</td>
    </tr>
  );
}
