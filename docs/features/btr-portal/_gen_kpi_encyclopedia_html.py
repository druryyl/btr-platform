# HTML shell, CSS, and renderer. Expects all KPI lists and helper k() to exist.
from html import escape

CSS = r"""
:root {
  --navy: #163A70;
  --navy-2: #0E2C56;
  --ink: #172337;
  --muted: #5B6B7C;
  --line: #D7E0E9;
  --paper: #FFFFFF;
  --canvas: #EEF2F6;
  --soft: #F6F8FA;
  --orange: #E07A1F;
  --green: #2F7D4A;
  --red: #B4453A;
  --gold: #C9A227;
  --card: #FFFFFF;
  --toc-w: 17.5rem;
  --radius: 12px;
  --shadow: 0 8px 28px rgba(22, 58, 112, .08);
  font-family: "Segoe UI", Calibri, Aptos, Arial, sans-serif;
}
* { box-sizing: border-box; }
html { scroll-behavior: smooth; }
body {
  margin: 0;
  background: var(--canvas);
  color: var(--ink);
  line-height: 1.55;
  font-size: 16px;
}
a { color: var(--navy); }
.skip {
  position: absolute; left: -999px; top: 0;
}
.skip:focus { left: 8px; top: 8px; background: #fff; padding: 8px; z-index: 20; }
.wrap {
  display: grid;
  grid-template-columns: 1fr;
  min-height: 100vh;
}
nav.toc {
  background: var(--navy-2);
  color: #E8EEF6;
  padding: 1.25rem 1rem 2rem;
}
nav.toc a { color: #D5E2F4; text-decoration: none; }
nav.toc a:hover, nav.toc a:focus { color: #fff; text-decoration: underline; }
nav.toc h2 {
  margin: 0 0 .75rem;
  font-size: .72rem;
  letter-spacing: .14em;
  text-transform: uppercase;
  color: var(--gold);
}
.toc-brand { font-weight: 700; font-size: 1.05rem; color: #fff; margin-bottom: .35rem; }
.toc-meta { font-size: .8rem; color: #9BB0C9; margin-bottom: 1rem; }
.toc-search {
  width: 100%;
  padding: .45rem .6rem;
  border: 0;
  border-radius: 6px;
  margin-bottom: 1rem;
  font: inherit;
}
.toc details { margin: .15rem 0; }
.toc summary {
  cursor: pointer;
  font-weight: 600;
  font-size: .92rem;
  color: #fff;
  padding: .25rem 0;
}
.toc ul { list-style: none; margin: .2rem 0 .6rem 0; padding: 0 0 0 .2rem; }
.toc li { margin: .18rem 0; font-size: .82rem; line-height: 1.35; }
.toc .sub { color: #F3D48A; font-size: .78rem; font-weight: 700; margin: .55rem 0 .2rem; }
main { padding: 0; }
.page {
  max-width: 52rem;
  margin: 0 auto;
  padding: 1.25rem 1rem 3rem;
}
header.cover {
  background: linear-gradient(165deg, var(--navy-2), var(--navy));
  color: #fff;
  padding: 2rem 1.25rem 1.6rem;
  border-radius: 0 0 18px 18px;
  margin: 0 0 1.25rem;
}
header.cover .kicker {
  color: var(--gold);
  font-size: .72rem;
  font-weight: 800;
  letter-spacing: .16em;
  text-transform: uppercase;
}
header.cover h1 {
  margin: .45rem 0 .6rem;
  font-size: 1.75rem;
  line-height: 1.15;
}
header.cover .lead { color: #D5E2F4; margin: 0 0 1.1rem; font-size: .98rem; }
.meta-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: .65rem;
}
.meta {
  background: rgba(255,255,255,.08);
  border: 1px solid rgba(255,255,255,.12);
  border-radius: 10px;
  padding: .7rem .85rem;
}
.meta dt { font-size: .7rem; letter-spacing: .08em; text-transform: uppercase; color: #F3D48A; margin: 0 0 .15rem; }
.meta dd { margin: 0; font-size: .95rem; }
h2.area {
  font-size: 1.45rem;
  color: var(--navy);
  margin: 2.2rem 0 .4rem;
  padding-top: .4rem;
}
h3.group {
  font-size: 1.15rem;
  color: var(--navy-2);
  margin: 1.6rem 0 .5rem;
}
h4.subg {
  font-size: 1rem;
  color: #3E5873;
  margin: 1.2rem 0 .45rem;
}
.intro, .overview, .questions, .exec {
  background: var(--paper);
  border: 1px solid var(--line);
  border-radius: var(--radius);
  padding: 1rem 1.05rem;
  margin: .75rem 0 1rem;
  box-shadow: var(--shadow);
}
.overview h3, .questions h3, .exec h3 { margin: .2rem 0 .5rem; color: var(--navy); font-size: 1.02rem; }
.overview ul, .questions ul, .exec ul, .kpi-card ul, .kpi-card ol { margin: .25rem 0 .2rem; padding-left: 1.15rem; }
.overview li, .questions li, .exec li { margin: .28rem 0; }
.note {
  font-size: .88rem;
  color: var(--muted);
  border-left: 3px solid var(--orange);
  padding: .15rem 0 .15rem .7rem;
  margin: .7rem 0 0;
}
aside {
  margin: .85rem 0 0;
  padding: .7rem .85rem;
  background: #FFF7EC;
  border-radius: 8px;
  font-size: .92rem;
}
.kpi-card {
  background: var(--card);
  border: 1px solid var(--line);
  border-radius: var(--radius);
  margin: .7rem 0 1rem;
  box-shadow: var(--shadow);
  break-inside: avoid;
  page-break-inside: avoid;
}
.kpi-card summary {
  cursor: pointer;
  list-style: none;
  display: grid;
  grid-template-columns: auto 1fr;
  gap: .55rem .75rem;
  align-items: start;
  padding: .9rem 1rem;
}
.kpi-card summary::-webkit-details-marker { display: none; }
.code {
  font-size: .68rem;
  font-weight: 700;
  letter-spacing: .04em;
  color: var(--navy);
  background: #E8EEF6;
  border-radius: 999px;
  padding: .18rem .55rem;
  white-space: nowrap;
  margin-top: .18rem;
}
.kpi-card h3 {
  margin: 0;
  font-size: 1.05rem;
  color: var(--navy-2);
  line-height: 1.3;
  overflow-wrap: anywhere;
}
.kpi-card .hint { font-size: .8rem; color: var(--muted); }
.kpi-body { padding: 0 1rem 1rem; border-top: 1px solid var(--line); }
.block { margin: .85rem 0; }
.block h4 {
  margin: 0 0 .3rem;
  font-size: .72rem;
  letter-spacing: .1em;
  text-transform: uppercase;
  color: var(--orange);
}
.read-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: .55rem;
}
.read {
  background: var(--soft);
  border-radius: 8px;
  padding: .65rem .75rem;
}
.read strong { display: block; font-size: .78rem; color: var(--navy); margin-bottom: .15rem; }
.related { display: flex; flex-wrap: wrap; gap: .35rem; }
.related a, .related span {
  background: #E8EEF6;
  color: var(--navy-2);
  border-radius: 999px;
  padding: .18rem .55rem;
  font-size: .8rem;
  text-decoration: none;
}
footer.doc {
  max-width: 52rem;
  margin: 0 auto;
  padding: 1rem 1rem 2.5rem;
  color: var(--muted);
  font-size: .82rem;
}
.sep { height: 1px; background: var(--line); margin: 1.5rem 0; }

@media (min-width: 900px) {
  .meta-grid { grid-template-columns: 1fr 1fr; }
  .read-grid { grid-template-columns: 1fr 1fr; }
  header.cover { padding: 2.6rem 2rem 2rem; border-radius: 0 0 24px 24px; }
  header.cover h1 { font-size: 2.15rem; }
  .page { padding: 1.5rem 1.25rem 3.5rem; }
}
@media (min-width: 1100px) {
  .wrap { grid-template-columns: var(--toc-w) 1fr; }
  nav.toc {
    position: sticky;
    top: 0;
    height: 100vh;
    overflow: auto;
    padding: 1.4rem 1.1rem 2rem;
  }
  main { min-width: 0; }
}
@media print {
  body { background: #fff; font-size: 11pt; }
  nav.toc { position: static; height: auto; overflow: visible; color: #000; background: #fff; border-bottom: 1px solid #ccc; }
  nav.toc a { color: #000; }
  .toc-search { display: none; }
  header.cover { break-after: page; color: #000; background: #fff; border: 1px solid #ccc; }
  header.cover .lead, header.cover .kicker, .meta dt { color: #333; }
  .kpi-card { box-shadow: none; }
  details { display: block; }
  details > *:not(summary) { display: block !important; }
}
"""

AREAS = [
    {
        "id": "customer",
        "num": "1",
        "name": "Customer",
        "intent": "Understand the health, risk, contribution, credit discipline, and collection actions of the customer book.",
        "purpose": "This area exists so management can see customers as both a source of omzet and a store of company cash (Piutang). It combines what is happening now, what may happen in the next 30 days, and what Finance and Sales should do today with each account.",
        "care": "A distribution business lives on repeat customers paying on time. When the book weakens, three losses arrive together: slower cash, quieter offtake, and credit given without approval. Totals on a finance report cannot show which relationships to grow, protect, collect, recover, or exit.",
        "decisions": [
            "Whether to continue, restrict, or stop supply to a customer.",
            "Where collection time should go today versus this week.",
            "Which relationships are strategic enough for owner involvement.",
            "Whether revenue growth is coming from a healthy book or from a shrinking, more concentrated book.",
            "When dormant and declining customers should be recovered versus released.",
        ],
        "questions": [
            "How healthy is the customer book over the next 30 days?",
            "How much receivable is outstanding, and how much of it is already late?",
            "Which customers create the highest cash and relationship risk?",
            "Is the book still buying, going quiet, or never converted?",
            "Are we collecting as fast as we are billing?",
            "Who should be contacted today, and is that work collection, credit, or sales recovery?",
            "Is credit policy actually being respected?",
        ],
        "groups": [
            ("1.1", "Kesehatan Portofolio Customer", [
                ("1.1.1", "Customer Risk Assessment", "CUSTOMER_111"),
                ("1.1.2", "Customer Lifecycle", "CUSTOMER_112"),
            ]),
            ("1.2", "Risiko Piutang & Collection", [
                ("1.2.1", "Piutang Exposure", "CUSTOMER_121"),
                ("1.2.2", "Collection Planning", "CUSTOMER_122"),
                ("1.2.3", "Collection Recovery", "CUSTOMER_123"),
            ]),
            ("1.3", "Kontribusi Customer Terhadap Bisnis", [
                ("1.3.1", "Customer Revenue Contribution", "CUSTOMER_131"),
                ("1.3.2", "Customer Exposure Concentration", "CUSTOMER_132"),
                ("1.3.3", "Strategic Customer Portfolio", "CUSTOMER_133"),
            ]),
            ("1.4", "Credit & Compliance", [
                ("1.4.1", "Credit Control", "CUSTOMER_141"),
                ("1.4.2", "Payment Discipline", "CUSTOMER_142"),
            ]),
            ("1.5", "Recovery Action Management", [
                ("1.5.1", "Collection Action Queue", "CUSTOMER_151"),
            ]),
        ],
        "exec": {
            "important": [
                "Portfolio Health Score and Portfolio Healthy % — weekly quality headlines.",
                "Total Piutang and Overdue Exposure — scale versus urgency of cash outstanding.",
                "Recovery vs Billing % — whether new sales are being turned back into cash.",
                "Strategic Customers At Risk Count — whether the important names are the weak names.",
                "Actions Today — whether the plan is becoming contact work.",
            ],
            "early": [
                "Customers At Risk Count and Risk Category - Watch.",
                "Inactivity Signal Count and Purchase Decline Signal Count — before 90-day dormant.",
                "Due Within 7 Days and Proactive Reminder Count — before overdue.",
                "Credit Limit Signal Count — before Plafond Breach Count.",
            ],
            "invest": [
                "Top Overdue Customers and Top 10 Outstanding Customers — names.",
                "Elevated Risk Receivable % — money attached to forecast risk.",
                "Signal-family counts — why the forecast fired.",
                "Action-type counts — who should work the account.",
                "Legacy Debt Count and Piutang > 90 Hari — chronic capital.",
            ],
            "order": [
                "Portfolio Health Score and Portfolio Healthy %.",
                "Total Piutang, then Overdue Exposure, then Piutang > 90 Hari.",
                "Customers At Risk, then High Risk / Critical, then Strategic At Risk.",
                "Top overdue and outstanding names.",
                "Recovery vs Billing %.",
                "Actions Today split into Immediate Collection, Reminders, Credit Review, Sales Recovery, Escalation.",
                "Lifecycle: Active, Declining, Dormant, Never Purchased.",
                "Credit: Plafond Breach and Suspended + Sales.",
            ],
        },
    },
    {
        "id": "salesman",
        "num": "2",
        "name": "Salesman",
        "intent": "Judge the sales force on plan attainment, field execution, customer-book quality, and capacity — not on omzet alone.",
        "purpose": "This area exists because company results are produced by people walking routes, taking orders, and owning customer books. Outcome (omzet, achievement, piutang) and execution (visits, GPS, orders) answer different questions and must be read together.",
        "care": "A few high-omzet salesmen can hide a team that misses plans, skips routes, and leaves dormant customers on the book. When that happens, next month’s omzet and next quarter’s piutang both deteriorate. Field activity without orders is theatre; orders without visits on the plan is unmanaged coverage.",
        "decisions": [
            "Who needs coaching this week, and for what — plan, route, conversion, or collection.",
            "Whether targets exist so performance can be judged.",
            "Whether the company depends too much on one salesman for omzet or piutang.",
            "Whether today’s field work covered the promised customers.",
            "Which books carry overdue that Sales must own with Finance.",
        ],
        "questions": [
            "How many salesmen are behind a real target, and how many have no target at all?",
            "Who is producing omzet, and is that production concentrated?",
            "Did the team walk the planned route?",
            "Did visits produce orders?",
            "Are check-ins physically credible?",
            "Which reps carry dormant customers or heavy overdue books?",
            "Is the active force large enough for the plan?",
        ],
        "groups": [
            ("2.1", "Pencapaian Target Penjualan", [
                ("2.1.1", "Sales Target Achievement", "SALESMAN_211"),
                ("2.1.2", "Revenue Production", "SALESMAN_212"),
            ]),
            ("2.2", "Produktivitas Lapangan", [
                ("2.2.1", "Visit Execution", "SALESMAN_221"),
                ("2.2.2", "Sales Call Effectiveness", "SALESMAN_222"),
                ("2.2.3", "Order Generation", "SALESMAN_223"),
            ]),
            ("2.3", "Kualitas Portofolio Customer", [
                ("2.3.1", "Customer Retention", "SALESMAN_231"),
                ("2.3.2", "Customer Exposure", "SALESMAN_232"),
            ]),
            ("2.4", "Ranking & Benchmark", [
                ("2.4.1", "Revenue Ranking", "SALESMAN_241"),
                ("2.4.2", "Productivity Ranking", "SALESMAN_242"),
                ("2.4.3", "Order Ranking", "SALESMAN_243"),
                ("2.4.4", "Collection Ranking", "SALESMAN_244"),
            ]),
            ("2.5", "Kapasitas Tim Sales", [
                ("2.5.1", "Sales Force Capacity", "SALESMAN_251"),
            ]),
        ],
        "exec": {
            "important": [
                "Below Target Count and Missing Target Setup Count.",
                "Visit Execution % and Effective Call Rate.",
                "Omzet Generated / Top 10 Omzet Ranking.",
                "High Overdue Exposure Count and Top Overdue Salesmen.",
                "Active Salesmen.",
            ],
            "early": [
                "Missing Target Setup Count at month start.",
                "Bottom Visit Execution and Missed Visits.",
                "GPS Valid Rate when execution looks too perfect.",
                "Dormant Portfolio Count before company dormant totals jump.",
            ],
            "invest": [
                "Principal Achievement Table — mix behind the headline.",
                "Bottom Effective Call Rate vs Bottom Visit Execution — conversion vs coverage.",
                "Top Orders vs Top Omzet — field pipeline vs invoiced result.",
                "High Piutang Exposure vs High Overdue Exposure — outstanding vs late.",
            ],
            "order": [
                "Active Salesmen and Missing Target Setup.",
                "Below Target Count and achievement ranking.",
                "Omzet ranking and Top Omzet Salesman %.",
                "Planned / Actual / Missed / Visit Execution %.",
                "Effective Calls, Effective Call Rate, GPS Valid Rate.",
                "Orders and Omzet Generated.",
                "Dormant Portfolio and overdue exposure by salesman.",
                "Bottom rankings for coaching names.",
            ],
        },
    },
    {
        "id": "item",
        "num": "3",
        "name": "Item",
        "intent": "See how much capital sits in stock, whether that stock is moving, whether active SKUs will run out, and what to buy, delay, transfer, or clear.",
        "purpose": "This area exists because inventory is cash in physical form. Composition tells where the money sits. Risk tells what is not selling. Forecast tells what will run out. Optimization tells what to do today. Those four questions must not be collapsed into one total.",
        "care": "A high inventory value can fund sales or hide dead stock. Buying more because the warehouse “feels empty” on a few holes, while slow movers age, is how distributors trap cash. Owners need to fill holes, delay excess, and recover idle capital as separate decisions.",
        "decisions": [
            "Whether to release purchase cash this week.",
            "Which categories and SKUs to clear versus replenish.",
            "Whether unposted purchases should be posted before new buys.",
            "When overstock and understock exist at the same time.",
            "Which idle SKUs deserve a recovery programme.",
        ],
        "questions": [
            "How much capital is in the warehouse, and in which categories?",
            "How much of that capital is slow, dead, or never sold?",
            "Will active SKUs run out within 30 days?",
            "Is the company overstocked, understocked, or both?",
            "What is the first action today — buy, delay, transfer, post, or clear?",
            "How much idle capital could still be recovered?",
        ],
        "groups": [
            ("3.1", "Nilai & Komposisi Persediaan", [
                ("3.1.1", "Inventory Valuation", "ITEM_311"),
                ("3.1.2", "Category Composition", "ITEM_312"),
                ("3.1.3", "Category Exposure", "ITEM_313"),
            ]),
            ("3.2", "Risiko Persediaan", [
                ("3.2.1", "Inventory Aging", "ITEM_321"),
                ("3.2.2", "Inventory Exposure", "ITEM_322"),
                ("3.2.3", "Critical Inventory", "ITEM_323"),
            ]),
            ("3.3", "Kesehatan & Forecast Persediaan", [
                ("3.3.1", "Inventory Health", "ITEM_331"),
                ("3.3.2", "Inventory Forecast", "ITEM_332"),
                ("3.3.3", "Inventory Coverage", "ITEM_333"),
            ]),
            ("3.4", "Optimisasi Inventory", [
                ("3.4.1", "Replenishment Planning", "ITEM_341"),
                ("3.4.2", "Capital Recovery", "ITEM_342"),
                ("3.4.3", "Inventory Action Queue", "ITEM_343"),
            ]),
        ],
        "exec": {
            "important": [
                "Total Inventory Value.",
                "At-Risk Inventory % and Aging Distribution.",
                "Inventory Health Score.",
                "Stock-Out Risk Items.",
                "Critical Actions Count.",
            ],
            "early": [
                "Slow Moving Count & Value — before Dead Stock.",
                "Never Sold Count & Value after new intake.",
                "Forecast Confidence when sizing buys.",
                "Average Days Of Supply hiding holes (always pair with Stock-Out Risk).",
            ],
            "invest": [
                "Top 10 Dead / Slow Moving — SKU names.",
                "Category Risk Exposure and Supplier Risk Exposure.",
                "Overstock / Understock Value together.",
                "Action Counts By Type.",
                "Recommended Purchase Budget versus Recoverable Capital.",
            ],
            "order": [
                "Total Inventory Value and Top Category %.",
                "Aging Distribution, then At-Risk %.",
                "Dead / Slow / Never Sold.",
                "Inventory Health Score.",
                "Stock-Out Risk vs Overstock / Understock.",
                "Item Days Of Supply and Recommended Purchase Qty.",
                "Recoverable Capital.",
                "Critical Actions split by type.",
            ],
        },
    },
    {
        "id": "supplier",
        "num": "4",
        "name": "Supplier",
        "intent": "See dependence on principals, the health of their stock, the scale of buying, and whether purchased goods actually become inventory.",
        "purpose": "This area exists because BTR buys from principals and holds their goods. Dependence is not only “we buy a lot from them”. It is also “we already hold a lot”, “what we hold is not moving”, and “what we bought is not yet posted”.",
        "care": "A distributor that depends on one principal for spend and for warehouse capital can lose range, price power, and cash at the same time. Unposted invoices mean the company has already spent (or committed) without gaining sellable stock. Inactivity mid-month can be a freeze or a stall — those are different decisions.",
        "decisions": [
            "Whether dependence on a principal is chosen strategy or accidental risk.",
            "Whether to keep buying a principal whose goods are already at risk in the warehouse.",
            "Whether to post existing invoices before placing new ones.",
            "Whether a quiet purchasing month is intentional.",
            "Which principal relationships need owner-level attention.",
        ],
        "questions": [
            "Who takes most of this month’s purchase cash?",
            "Who already holds most of the warehouse capital?",
            "Which principals dominate buying and stock (or sick stock) together?",
            "Are purchased goods posted into inventory?",
            "Is there an aged posting backlog?",
            "Has purchasing gone quiet too late in the month?",
        ],
        "groups": [
            ("4.1", "Ketergantungan Supplier", [
                ("4.1.1", "Supplier Dependency", "SUPPLIER_411"),
                ("4.1.2", "Principal Dependency", "SUPPLIER_412"),
                ("4.1.3", "Dependency Risk", "SUPPLIER_413"),
            ]),
            ("4.2", "Risiko Supplier", [
                ("4.2.1", "Supplier Risk Monitoring", "SUPPLIER_421"),
                ("4.2.2", "Supplier Ranking", "SUPPLIER_422"),
            ]),
            ("4.3", "Kontribusi Pembelian", [
                ("4.3.1", "Purchase Volume", "SUPPLIER_431"),
                ("4.3.2", "Principal Contribution", "SUPPLIER_432"),
                ("4.3.3", "Purchasing Summary", "SUPPLIER_433"),
            ]),
            ("4.4", "Kualitas Operasional Purchasing", [
                ("4.4.1", "Purchase Processing", "SUPPLIER_441"),
                ("4.4.2", "Purchasing Backlog", "SUPPLIER_442"),
                ("4.4.3", "Purchasing Activity", "SUPPLIER_443"),
            ]),
        ],
        "exec": {
            "important": [
                "Top 1 Principal % and Top Supplier % — spend versus stock dependence.",
                "Compound Dependency Count.",
                "Grand Total Purchase.",
                "Posted % and Qualified Backlog Count & Value.",
                "Principal At Risk Count.",
            ],
            "early": [
                "Purchasing Inactivity Flag after mid-month.",
                "Pending Posting Value before it becomes qualified backlog.",
                "Supplier Risk Exposure rising on a high-spend principal.",
            ],
            "invest": [
                "Principal Exposure Comparison — names across spend, stock, and at-risk.",
                "Top 10 Principal Ranking versus Top 10 Supplier Ranking.",
                "Qualified Backlog on the same names as stock-out SKUs.",
            ],
            "order": [
                "Grand Total Purchase and Total Invoice.",
                "Top 1 / Top 3 Principal % and Top Principal %.",
                "Top Supplier % and Top 10 Supplier Ranking.",
                "Compound Dependency and Principal Exposure Comparison.",
                "Principal At Risk Count and Supplier Risk Exposure.",
                "Posted %, Pending Posting, Qualified Backlog.",
                "Purchasing Inactivity Flag.",
            ],
        },
    },
]


def paras(items):
    return "".join(f"<p>{escape(p)}</p>" for p in items)


def lis(items, ordered=False):
    tag = "ol" if ordered else "ul"
    inner = "".join(f"<li>{escape(i)}</li>" for i in items)
    return f"<{tag}>{inner}</{tag}>"


def related_html(names, index):
    bits = []
    for n in names:
        target = index.get(n)
        if target:
            bits.append(f'<a href="#{target}">{escape(n)}</a>')
        else:
            bits.append(f"<span>{escape(n)}</span>")
    return '<div class="related">' + "".join(bits) + "</div>"


def render_kpi(kpi, index):
    how = kpi["how"]
    return f"""
<article class="kpi-card" id="{kpi['id']}">
  <details>
    <summary>
      <span class="code">{escape(kpi['code'])}</span>
      <div>
        <h3>{escape(kpi['name'])}</h3>
        <div class="hint">Tap to open the owner briefing</div>
      </div>
    </summary>
    <div class="kpi-body">
      <div class="block">
        <h4>What it means</h4>
        {paras(kpi['means'])}
      </div>
      <div class="block">
        <h4>Why it matters</h4>
        {paras(kpi['matters'] if isinstance(kpi['matters'], list) else [kpi['matters']])}
      </div>
      <div class="block">
        <h4>How to read</h4>
        <div class="read-grid">
          <div class="read"><strong>High</strong>{escape(how['high'])}</div>
          <div class="read"><strong>Low</strong>{escape(how['low'])}</div>
        </div>
      </div>
      <div class="block">
        <h4>Warning signs</h4>
        {lis(kpi['warnings'])}
        <p class="note">No extra numeric trigger is stated here unless the source dashboards already use one.</p>
      </div>
      <div class="block">
        <h4>Related KPIs</h4>
        {related_html(kpi['related'], index)}
      </div>
      <div class="block">
        <h4>Next KPI to check</h4>
        {lis(kpi['next'], ordered=True)}
      </div>
      <div class="block">
        <h4>Typical owner action</h4>
        {lis(kpi['actions'])}
      </div>
      <div class="block">
        <h4>Common misinterpretations</h4>
        <p>{escape(kpi['misread'])}</p>
      </div>
    </div>
  </details>
</article>
"""


def build_index(ns):
    idx = {}
    for area in AREAS:
        for _g, _gn, subs in area["groups"]:
            for _s, _sn, key in subs:
                for kpi in ns[key]:
                    idx[kpi["name"]] = kpi["id"]
    return idx


def toc_html(ns):
    chunks = [
        '<p class="toc-brand">KPI Encyclopedia</p>',
        '<p class="toc-meta">Customer · Salesman · Item · Supplier</p>',
        '<label class="skip" for="tocFilter">Filter</label>',
        '<input class="toc-search" id="tocFilter" type="search" placeholder="Find a KPI…" oninput="filterToc(this.value)">',
        '<h2>On this page</h2>',
        '<ul>',
        '<li><a href="#cover">Cover</a></li>',
        '<li><a href="#how-to-use">How to use</a></li>',
        '<li><a href="#contents">Contents</a></li>',
        '</ul>',
    ]
    for area in AREAS:
        chunks.append(f'<details><summary>{escape(area["num"] + ". " + area["name"])}</summary>')
        chunks.append("<ul>")
        chunks.append(f'<li><a href="#{area["id"]}-overview">Overview</a></li>')
        chunks.append(f'<li><a href="#{area["id"]}-questions">Business questions</a></li>')
        for gcode, gname, subs in area["groups"]:
            chunks.append(f'<li class="sub">{escape(gcode + " " + gname)}</li>')
            for scode, sname, key in subs:
                chunks.append(f'<li><a href="#{key.lower()}">{escape(scode + " " + sname)}</a></li>')
                for kpi in ns[key]:
                    chunks.append(
                        f'<li><a href="#{kpi["id"]}">{escape(kpi["code"] + " " + kpi["name"])}</a></li>'
                    )
        chunks.append(f'<li><a href="#{area["id"]}-exec">Executive summary</a></li>')
        chunks.append("</ul></details>")
    chunks.append('<ul><li><a href="#exec-all">Executive summary</a></li><li><a href="#closing">Closing note</a></li></ul>')
    return "".join(chunks)


def area_html(area, ns, index):
    d = area
    parts = [
        f'<section id="{d["id"]}">',
        f'<h2 class="area">{escape(d["num"] + ". " + d["name"])}</h2>',
        f'<section class="overview" id="{d["id"]}-overview">',
        "<h3>Purpose</h3>",
        f'<p>{escape(d["purpose"])}</p>',
        "<h3>Why the owner should care</h3>",
        f'<p>{escape(d["care"])}</p>',
        "<h3>Typical decisions</h3>",
        lis(d["decisions"]),
        f'<aside><p><strong>Owner lens.</strong> {escape(d["intent"])}</p></aside>',
        "</section>",
        f'<section class="questions" id="{d["id"]}-questions">',
        "<h3>Business questions this area answers</h3>",
        lis(d["questions"]),
        "</section>",
    ]
    for gcode, gname, subs in d["groups"]:
        parts.append(f'<h3 class="group" id="g-{gcode.replace(".", "-")}">{escape(gcode + " " + gname)}</h3>')
        for scode, sname, key in subs:
            parts.append(f'<h4 class="subg" id="{key.lower()}">{escape(scode + " " + sname)}</h4>')
            for kpi in ns[key]:
                parts.append(render_kpi(kpi, index))
    e = d["exec"]
    parts += [
        f'<section class="exec" id="{d["id"]}-exec">',
        f'<h3>Executive summary — {escape(d["name"])}</h3>',
        "<h4>Most important KPIs</h4>",
        lis(e["important"]),
        "<h4>Early warning KPIs</h4>",
        lis(e["early"]),
        "<h4>Investigation KPIs</h4>",
        lis(e["invest"]),
        "<h4>Recommended reading order</h4>",
        lis(e["order"], ordered=True),
        "</section>",
        "</section>",
        '<div class="sep"></div>',
    ]
    return "".join(parts)


JS = r"""
function filterToc(q) {
  q = (q || '').toLowerCase();
  document.querySelectorAll('nav.toc details li').forEach(function (li) {
    var t = li.textContent.toLowerCase();
    li.style.display = !q || t.indexOf(q) !== -1 ? '' : 'none';
  });
}
window.addEventListener('beforeprint', function () {
  document.querySelectorAll('details').forEach(function (d) { d.setAttribute('open', ''); });
});
"""


def render_document(ns):
    index = build_index(ns)
    areas = "".join(area_html(a, ns, index) for a in AREAS)
    return f"""<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">
  <title>BTR KPI Encyclopedia — Customer, Salesman, Item, Supplier</title>
  <style>{CSS}</style>
</head>
<body>
  <a class="skip" href="#cover">Skip to content</a>
  <div class="wrap">
    <nav class="toc" aria-label="Table of contents">
      {toc_html(ns)}
    </nav>
    <main>
      <header class="cover" id="cover">
        <div class="kicker">Business handbook</div>
        <h1>KPI Encyclopedia</h1>
        <p class="lead">This handbook is the primary reference when an owner sees a KPI on a BTR Portal dashboard and needs to know what it means, why it matters, how to read it, when to worry, what to check next, and what to do. It is not a technical, database, or formula manual.</p>
        <div class="meta-grid">
          <div class="meta"><dt>Entity name</dt><dd>CV. Bintang Timur Rahayu (BTR)</dd></div>
          <div class="meta"><dt>Business intent</dt><dd>Help owners, directors, and general managers use dashboard KPIs for decision making across Customer, Salesman, Item, and Supplier.</dd></div>
          <div class="meta"><dt>Data source</dt><dd>BTR Portal management dashboards (read-only analytics from BTR operational data). Desktop remains the place where transactions are entered and operational actions are completed.</dd></div>
          <div class="meta"><dt>Generated date</dt><dd>2 September 2026</dd></div>
        </div>
      </header>
      <div class="page">
        <section class="intro" id="how-to-use">
          <h2>How to use this handbook</h2>
          <p>Open the dashboard, find the KPI name, then find the same name here. Read <strong>What it means</strong> and <strong>How to read</strong> first. If the number looks wrong for the business, follow <strong>Next KPI to check</strong> before changing credit, routes, or purchasing.</p>
          <p>KPI names match the portal as closely as possible. Indonesian business words used in BTR — Piutang, Omzet, Faktur, Plafond, Principal, Wilayah — are kept because that is how the business speaks.</p>
          <p>Where a dashboard does not publish a numeric trigger, this handbook does not invent one. Direction (high/low) is a general reading, not a target. Forecast views look 30 days ahead using business rules; they are not automatic credit holds, purchase orders, or write-offs.</p>
        </section>
        <section class="intro" id="contents">
          <h2>Contents</h2>
          <ol>
            <li><a href="#customer">Customer</a> — portfolio health, piutang, contribution, credit, collection actions.</li>
            <li><a href="#salesman">Salesman</a> — targets, field productivity, book quality, rankings, capacity.</li>
            <li><a href="#item">Item</a> — inventory value, aging risk, forecast, replenishment actions.</li>
            <li><a href="#supplier">Supplier</a> — principal dependence, supplier risk, purchase contribution, posting quality.</li>
          </ol>
        </section>
        {areas}
        <section class="exec" id="exec-all">
          <h2>Executive summary — all four areas</h2>
          <p>Read Customer for cash and relationship risk, Salesman for whether people are producing and covering, Item for whether warehouse capital is working, and Supplier for whether buying dependence and posting are under control. Problems that look like “sales is down” often start as missed visits, declining customers, or stock-outs on the SKUs that actually sell.</p>
          <h3>Most important KPIs</h3>
          <ul>
            <li>Portfolio Health Score, Total Piutang, Overdue Exposure, Recovery vs Billing %.</li>
            <li>Below Target Count, Visit Execution %, Effective Call Rate.</li>
            <li>Total Inventory Value, At-Risk Inventory %, Stock-Out Risk Items.</li>
            <li>Top 1 Principal %, Compound Dependency Count, Qualified Backlog Count &amp; Value.</li>
          </ul>
          <h3>Early warning KPIs</h3>
          <ul>
            <li>Watch category, Inactivity Signal, Due Within 7 Days, Credit Limit Signal.</li>
            <li>Missing Target Setup, Bottom Visit Execution, GPS Valid Rate.</li>
            <li>Slow Moving Count &amp; Value, Never Sold after new intake.</li>
            <li>Purchasing Inactivity Flag, Pending Posting Value.</li>
          </ul>
          <h3>Investigation KPIs</h3>
          <ul>
            <li>Named lists: Top Overdue Customers, Top 10 Outstanding Customers, Top 10 Dead / Slow Moving, Principal Exposure Comparison.</li>
            <li>Action queues: Collection Action Queue and Inventory Action Counts By Type.</li>
            <li>Salesman books: Top Overdue Salesmen, Dormant Portfolio Count.</li>
          </ul>
          <h3>Recommended reading order for a morning scan</h3>
          <ol>
            <li>Customer health and overdue quality (not only Total Piutang).</li>
            <li>Collection action queue and Recovery vs Billing %.</li>
            <li>Salesman below-target and visit execution.</li>
            <li>Inventory at-risk share and stock-out list.</li>
            <li>Supplier concentration and qualified posting backlog.</li>
          </ol>
        </section>
        <section class="intro" id="closing">
          <h2>Closing note</h2>
          <p>After a KPI becomes a concern, the portal’s job is to explain and prioritise. The operational job — calling the customer, posting the invoice, changing the visit plan, placing or withholding a purchase — is completed in BTR Desktop and in the field. Use this handbook so those actions are aimed at the right problem.</p>
        </section>
      </div>
      <footer class="doc">
        KPI Encyclopedia · CV. Bintang Timur Rahayu · Source: BTR Portal · 2 September 2026 · For owners and senior management
      </footer>
    </main>
  </div>
  <script>{JS}</script>
</body>
</html>
"""
