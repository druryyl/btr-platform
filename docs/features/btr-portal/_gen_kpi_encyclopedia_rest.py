# Continuation data + HTML renderer. Executed after the customer KPI lists exist.

SALESMAN_211 = [
    k(
        "2.1.1.1",
        "Below Target Count",
        "How many salesmen have a monthly target configured and are currently in the Warning band (80–99% of target) or the Critical band (below 80%). Salesmen with no target are not counted here.",
        "This is the coaching and intervention queue. It answers who is behind plan, not who lacks a plan.",
        {
            "high": "Many reps are missing plan. Sales management time should go to coaching, coverage, and pipeline — not only to celebrating top omzet.",
            "low": "Few reps are behind a configured target. Check Missing Target Setup Count before concluding the team is healthy.",
        },
        [
            "Count rising mid-month while company Achievement still looks acceptable — a few stars may be covering a weak team.",
            "The same names remaining Below Target across months.",
        ],
        ["Missing Target Setup Count", "Top 10 Achievement % Ranking", "Principal Achievement Table", "Omzet Generated"],
        [
            "Open Top 10 Achievement % Ranking and the named below-target reps.",
            "Check Missing Target Setup Count so you are not mixing “no target” with “missed target”.",
            "Check visit execution for those reps — outcome vs activity.",
        ],
        [
            "Sales manager coaches below-target reps this week.",
            "Do not judge reps who have no target using this KPI.",
        ],
        "Unknown (no target) is not a performance judgment. Below Target only includes reps who have a target and are in Warning or Critical.",
    ),
    k(
        "2.1.1.2",
        "Missing Target Setup Count",
        "How many salesmen are commercially active this month — they have omzet or invoiced customers — but have no target configured. Their achievement cannot be judged.",
        "This is a planning gap, not a sales-skill gap. Without targets, Below Target Count understates the real performance problem.",
        {
            "high": "A large part of the team cannot be managed against plan. Achievement % at company level may also be incomplete.",
            "low": "Active reps almost all have targets. Performance bands are usable.",
        },
        [
            "Any count remaining after month start.",
            "Active high-omzet reps appearing here — the biggest books have no plan.",
        ],
        ["Below Target Count", "Active Salesmen", "Top 10 Achievement % Ranking"],
        [
            "Name the reps and set targets in the operational system.",
            "Until then, manage them on omzet and activity, not on Achievement %.",
        ],
        [
            "Sales admin/manager configures missing targets immediately.",
            "Do not treat a blank achievement as 0% failure or as 100% success.",
        ],
        "Missing target is a planning incompleteness signal. It is not “below target”.",
    ),
    k(
        "2.1.1.3",
        "Top 10 Achievement % Ranking",
        "The ten salesmen with the highest achievement percentage this month (among those with a target). This ranking is about plan attainment, not about who billed the most money.",
        "A rep can lead omzet and still miss target, or lead achievement on a small target. Owners should read this list next to Top 10 Omzet Ranking.",
        {
            "high": "Read as a ranking. Names at the top are beating plan.",
            "low": "If even the tenth name is below 100%, the team is broadly behind plan — confirm with Below Target Count.",
        },
        [
            "Top achievement names who have weak visit execution — outcome without coverage discipline, or a target that is too easy. Target quality is not scored here; treat easy targets as a management question, not as a KPI fact.",
            "High omzet names missing from this list.",
        ],
        ["Below Target Count", "Top 10 Omzet Ranking", "Principal Achievement Table"],
        [
            "Compare with Top 10 Omzet Ranking.",
            "Check Principal Achievement Table for mix.",
            "Check visit execution for outliers.",
        ],
        [
            "Coach from both lists: money producers and plan attainers are not always the same people.",
        ],
        "Achievement % is blank or unknown when target is missing. Those reps will not rank here.",
    ),
    k(
        "2.1.1.4",
        "Principal Achievement Table",
        "Each salesman’s invoiced omzet against target, broken down by principal (supplier). It shows who is selling whose products, and which principal-level plans are being missed.",
        "Company achievement can look acceptable while one principal is collapsing on several reps. This table is the mix view behind the headline.",
        {
            "high": "Not a single directional number. Read cells: green attainment vs empty/missing vs far behind.",
            "low": "Rows of missed principal targets mean those brands are not getting field push.",
        },
        [
            "A principal missed across many salesmen — a range or supply issue, not one lazy rep.",
            "One salesman missing every principal — a coverage or capability issue.",
        ],
        ["Below Target Count", "Top 10 Achievement % Ranking", "Top 1 Principal %"],
        [
            "Identify the weakest principal columns.",
            "Check inventory availability for that principal on the Item branch.",
            "Check salesman visit focus.",
        ],
        [
            "Sales manager realigns focus by principal.",
            "Purchasing/owner checks supply if many reps miss the same principal.",
        ],
        "Principal here means supplier. Achievement still uses invoiced Faktur omzet, not field orders.",
    ),
]

SALESMAN_212 = [
    k(
        "2.1.2.1",
        "Omzet Generated",
        "Invoiced sales produced in the field-activity period being viewed. On field dashboards this sits next to visits and orders so management can see whether activity became billing.",
        "It connects execution to outcome. Visits and orders that never become Faktur omzet have not finished the commercial cycle.",
        {
            "high": "The selected team or day produced strong invoiced sales.",
            "low": "Little invoiced sales from the activity being reviewed.",
        },
        [
            "Omzet high while Visit Execution % is low — results without route discipline, or a plan that does not match how they actually sell.",
            "Orders Generated high while Omzet Generated is low — orders are not converting to invoices (fulfilment/process; detail of that gap is not a KPI in this branch).",
        ],
        ["Sales Orders", "Orders Generated", "Top 10 Omzet Ranking", "Effective Calls"],
        [
            "Check Orders Generated vs this omzet.",
            "Check Effective Call Rate.",
            "Check company Top 10 Omzet Ranking.",
        ],
        [
            "If activity is high and omzet is low, coach conversion and order quality, not more random visits.",
        ],
        "Field omzet follows invoiced Faktur attribution. Orders Generated counts field orders, which are not the same document as Faktur.",
    ),
    k(
        "2.1.2.2",
        "Top Omzet Salesman %",
        "The share of company month-to-date invoiced omzet coming from the single highest-billing salesman. It is team revenue concentration.",
        "If one rep produces a large share, illness, resignation, or a bad month for that person is a company event. No automatic threshold is defined — read the share as dependence information.",
        {
            "high": "The team’s billing depends heavily on one person.",
            "low": "Billing is spread across more of the force.",
        },
        [
            "Share rising while Active Salesmen is stable — the rest of the team is contributing less.",
            "The top omzet salesman also carrying High Overdue Exposure.",
        ],
        ["Top 10 Omzet Ranking", "Top 10 Salesman (Omzet)", "Active Salesmen", "Top Piutang Salesman %"],
        [
            "Open Top 10 Omzet Ranking.",
            "Check Below Target Count for the rest of the team.",
        ],
        [
            "Develop the next tier of reps so the company is not a one-person revenue engine.",
            "Protect the top rep’s customer book (credit and retention).",
        ],
        "Informational concentration — no automatic warning threshold.",
    ),
    k(
        "2.1.2.3",
        "Top 10 Omzet Ranking",
        "The ten salesmen with the highest invoiced omzet this month on the Salesmen dashboard. This is who produced the money.",
        "Use it for recognition and for risk: the top of this list holds customer relationships the company cannot afford to neglect.",
        {
            "high": "Read as a ranking of money producers.",
            "low": "A weak tenth place means a small productive core.",
        },
        [
            "Top omzet names with poor visit execution or poor GPS Valid Rate — outcome quality vs activity integrity.",
            "Top omzet names with High Overdue Exposure — they may be selling faster than they collect.",
        ],
        ["Top 10 Salesman (Omzet)", "Top Omzet Salesman %", "Top 10 Achievement % Ranking", "High Overdue Exposure Count"],
        [
            "Compare with achievement ranking.",
            "Check each name’s overdue and dormant portfolio.",
        ],
        [
            "Coach the bottom of the productive list and protect the top.",
        ],
    ),
]

SALESMAN_221 = [
    k(
        "2.2.1.1",
        "Planned Visits",
        "How many customers were on the effective visit plan for the salesman-day (or the team, on the overview). This is the expected coverage — the denominator for execution.",
        "Without a plan, Visit Execution % cannot be judged. A low planned count can mean a light route or a missing plan, not a lazy day.",
        {
            "high": "A heavy planned coverage. Finishing it is a full day’s work.",
            "low": "A thin plan. High execution % on a tiny plan is not a strong coverage story.",
        },
        [
            "Planned visits of zero for an active salesman — planning gap.",
            "Plan much smaller than the salesman’s customer book.",
        ],
        ["Actual Visits", "Missed Visits", "Visit Execution %", "Active Salesmen"],
        [
            "Check Actual Visits and Missed Visits.",
            "If planned is zero, fix visit planning before judging execution.",
        ],
        [
            "Supervisors must publish a real plan. You cannot manage route discipline without one.",
        ],
        "Field activity for a salesman-day is a live operational view, not the same snapshot cadence as sales/piutang dashboards.",
    ),
    k(
        "2.2.1.2",
        "Actual Visits",
        "How many distinct customers the salesman (or team) checked in with that day. Multiple check-ins at the same customer still count as one actual visit.",
        "This is whether the field showed up. It is activity, not yet productivity — Effective Calls tells you whether the visit produced an order.",
        {
            "high": "Broad physical coverage that day.",
            "low": "Few customers visited. Check missed visits and whether the plan was realistic.",
        },
        [
            "Actual far below Planned.",
            "Actual high because of Unplanned Visits while Missed Visits stay high — they are substituting, not covering.",
        ],
        ["Planned Visits", "Missed Visits", "Unplanned Visits", "Effective Calls", "GPS Valid Rate"],
        [
            "Check Missed Visits for who was skipped.",
            "Check GPS Valid Rate for whether check-ins look authentic.",
            "Check Effective Call Rate.",
        ],
        [
            "Supervisor reviews missed names the same day.",
            "Do not praise actual visit volume if GPS is weak or orders are zero.",
        ],
    ),
    k(
        "2.2.1.3",
        "Missed Visits",
        "Planned customers with no check-in that day. These are coverage gaps — the route was promised and not executed.",
        "Missed visits are where pipeline and collection both leak. Repeat missed names become declining and then dormant customers.",
        {
            "high": "The plan is not being walked. Customers on the route are not seeing the company.",
            "low": "Almost all planned customers were visited.",
        },
        [
            "The same customers missed across days.",
            "Missed visits on overdue or declining customers.",
        ],
        ["Planned Visits", "Actual Visits", "Visit Execution %", "Dormant Portfolio Count"],
        [
            "List the missed customers.",
            "Check whether they are overdue or declining on the Customer branch.",
        ],
        [
            "Require a reason and a catch-up date for missed strategic or overdue customers.",
            "Fix plans that are systematically unfinishable.",
        ],
    ),
    k(
        "2.2.1.4",
        "Unplanned Visits",
        "Check-ins at customers who were not on the effective plan. This can be good ad-hoc selling or a sign that the plan does not match how the salesman actually works.",
        "Unplanned volume is not automatically bad. It becomes a concern when Missed Visits stay high — the salesman is replacing the route rather than extending it.",
        {
            "high": "A lot of off-plan activity. Read together with missed visits.",
            "low": "The salesman mostly walked the published route.",
        },
        [
            "Unplanned high and Missed high together.",
            "Unplanned visits with weak GPS Valid Rate.",
        ],
        ["Actual Visits", "Missed Visits", "Effective Calls", "Visit Execution %"],
        [
            "See which unplanned customers produced Effective Calls.",
            "See which planned customers were sacrificed.",
        ],
        [
            "If off-plan visits are productive, update the route template.",
            "If they are not productive, bring the salesman back to the plan.",
        ],
        "Unplanned is not the same as invalid. GPS Valid Rate is the authenticity check; Unplanned is the plan-deviation check.",
    ),
    k(
        "2.2.1.5",
        "Visit Execution %",
        "Actual visits divided by planned visits. It is route compliance — did the team walk the published coverage? If there is no plan, this measure is not applicable.",
        "This is the primary field-discipline KPI. Company sales can still look fine while execution is poor — that usually shows up later as dormant customers.",
        {
            "high": "The plan was largely completed.",
            "low": "Coverage promises are not being kept.",
        },
        [
            "Low execution on several salesmen the same day — a systemic planning or weather/operations issue, or a supervision issue.",
            "High execution with very low Planned Visits — a trivial plan.",
        ],
        ["Planned Visits", "Actual Visits", "Missed Visits", "Top Visit Execution", "Bottom Visit Execution"],
        [
            "Check Bottom Visit Execution for names.",
            "Check Effective Call Rate — compliance without orders is incomplete.",
        ],
        [
            "Supervisors enforce catch-up on low-execution reps.",
            "Do not confuse execution with sales success; read both lenses.",
        ],
        "Team execution uses the sum of actuals over the sum of planned, not an average of personal percentages. A busy salesman weighs more than a light one.",
    ),
]

SALESMAN_222 = [
    k(
        "2.2.2.1",
        "Effective Calls",
        "How many visits produced at least one sales order on the same day for that customer. A visit without an order is activity; an effective call is a productive visit.",
        "Owners use it to see whether the field is selling or only touring. It is the bridge from check-ins to order generation.",
        {
            "high": "Many visits created orders that day.",
            "low": "Few visits converted. Either the book is not buying, or the calls are courtesy visits.",
        },
        [
            "Actual Visits high and Effective Calls low.",
            "Effective Calls high and Omzet Generated low — orders not yet invoiced, or small orders.",
        ],
        ["Effective Call Rate", "Sales Orders", "Actual Visits", "Orders Generated"],
        [
            "Check Effective Call Rate.",
            "Check Sales Orders / Orders Generated.",
            "Check GPS Valid Rate so “effective” visits are also physically credible.",
        ],
        [
            "Coach call quality and order-taking, not only kilometres.",
        ],
        "Effective Call uses field sales orders, not Faktur. A productive call can still fail later in invoicing.",
    ),
    k(
        "2.2.2.2",
        "Effective Call Rate",
        "Effective calls divided by actual visits. It is visit productivity — of the customers who were seen, how many produced an order that day?",
        "A high Visit Execution % with a low Effective Call Rate means the team is obedient and unproductive. A high rate with low execution means they sell where they go but skip the route.",
        {
            "high": "Visits are converting.",
            "low": "Visits are not converting.",
        },
        [
            "Rate falling while execution stays high.",
            "Bottom Effective Call Rate names who also have dormant portfolios.",
        ],
        ["Effective Calls", "Visit Execution %", "Top Effective Call Rate", "Bottom Effective Call Rate"],
        [
            "Open Bottom Effective Call Rate.",
            "Check order counts and omzet for those reps.",
        ],
        [
            "Coach conversion on low-rate reps; do not only add more visits.",
        ],
    ),
    k(
        "2.2.2.3",
        "GPS Valid Rate",
        "The share of check-ins whose GPS coordinates sit close enough to the customer location to be treated as valid. Documented bands: Valid at 50 metres or closer, Warning between 50 and 100 metres, Suspicious beyond 100 metres.",
        "This is a visit-authenticity and data-quality signal. It does not measure sales skill. Weak GPS makes execution KPIs less trustworthy.",
        {
            "high": "Most check-ins look physically credible.",
            "low": "Many check-ins are far from the customer. Coverage numbers may be inflated.",
        },
        [
            "Low valid rate on a salesman with perfect Visit Execution %.",
            "Suspicious check-ins repeating at the same names.",
        ],
        ["Actual Visits", "Visit Execution %", "Effective Calls"],
        [
            "Review the salesman’s map/replay for the day.",
            "Do not use execution rankings alone until GPS is acceptable.",
        ],
        [
            "Supervisor confronts suspicious patterns.",
            "Fix customer coordinates if the master location is wrong — bad master data also produces invalid GPS.",
        ],
        "GPS Valid Rate is authenticity of presence, not quality of the sales conversation.",
    ),
]

SALESMAN_223 = [
    k(
        "2.2.3.1",
        "Sales Orders",
        "How many field sales orders were taken in the selected field-activity view (salesman-day detail and overview). This is order documents from the mobile sales process, not invoiced Faktur count.",
        "It is the pipeline created in the field that day. Management uses it to see whether visits produced demand, before asking whether warehouse and billing completed the cycle.",
        {
            "high": "The field booked a lot of orders.",
            "low": "Few orders from the activity reviewed.",
        },
        [
            "Sales Orders low while Actual Visits are high.",
            "Sales Orders high while Omzet Generated stays low across subsequent days — conversion to Faktur may be stalling. The exact fulfilment gap is not a KPI in this branch.",
        ],
        ["Orders Generated", "Effective Calls", "Omzet Generated"],
        [
            "Check Effective Calls — orders should line up with productive visits.",
            "Check Omzet Generated for invoiced outcome.",
        ],
        [
            "If orders exist but invoices do not follow, take the issue to operations — this dashboard will not complete fulfilment.",
        ],
        "Sales Orders are not Faktur. Do not compare this count to Total Faktur on the Sales dashboard as if they were the same document.",
    ),
    k(
        "2.2.3.2",
        "Orders Generated",
        "Field-order production on the Sales Force Overview — the team-level order count (and, on that surface, related order value) from the mobile order book, not from Faktur omzet.",
        "Owners scanning the whole force use this to see whether the organisation created demand today, alongside visit execution.",
        {
            "high": "The team generated substantial field orders.",
            "low": "Little order creation across the force.",
        },
        [
            "Orders Generated weak while Visit Execution % looks fine — compliance without selling.",
            "Orders concentrated in one or two salesmen — see Top Orders.",
        ],
        ["Sales Orders", "Top Orders", "Omzet Generated", "Effective Call Rate"],
        [
            "Check Top Orders ranking.",
            "Check Effective Call Rate at team level.",
        ],
        [
            "Coach low-order reps; protect the process that converts orders to Faktur.",
        ],
        "On the overview, order value is field-order value, not invoiced omzet. Do not add it to Total MTD Omzet.",
    ),
]

SALESMAN_231 = [
    k(
        "2.3.1.1",
        "Dormant Portfolio Count",
        "How many salesmen have at least one dormant customer (90 days without a Faktur, with prior history) still attributed to their book — usually via last invoicing salesman.",
        "This is account-maintenance failure by rep. Dormant customers on a book are lost run-rate that still “belongs” to someone.",
        {
            "high": "Many reps are carrying silent customers. Recovery work is scattered.",
            "low": "Few reps have dormant names — either the book is live or dormant accounts were reassigned/exited.",
        },
        [
            "The same reps appearing here and on Below Target Count.",
            "Dormant portfolios on top omzet salesmen — even stars leak accounts.",
        ],
        ["Dormant Customer Count", "Sales Recovery Count", "Missed Visits", "Below Target Count"],
        [
            "Open the salesman’s dormant names on Customer Analytics.",
            "Check visit plans — are those customers even scheduled?",
        ],
        [
            "Require a recover-or-exit plan per dormant name on the book.",
            "Do not keep paying attention only to this month’s invoiced customers.",
        ],
    ),
]

SALESMAN_232 = [
    k(
        "2.3.2.1",
        "High Piutang Exposure Count",
        "How many salesmen sit in the top 20% of the team by open receivable, among reps who have a balance. These books hold a disproportionate share of company piutang.",
        "Receivable risk is not only a Finance issue. The invoicing salesman owns the customer relationship that created the debt.",
        {
            "high": "Many reps (relatively) carry heavy books — or the team is small and the top 20% is a few names. Read the ranking, not only the count.",
            "low": "Fewer reps dominate open balance.",
        },
        [
            "High piutang exposure without matching omzet — old debt sitting on the book.",
            "High exposure plus Below Target — they sold on credit and are not replacing it with new clean sales.",
        ],
        ["Top Piutang Salesman %", "High Overdue Exposure Count", "Total Piutang"],
        [
            "See which reps and how much they hold.",
            "Check High Overdue Exposure Count — outstanding is not always overdue.",
        ],
        [
            "Give those reps a collection partnership with Finance.",
            "Review credit they continue to extend.",
        ],
        "The 20% cut is relative to other reps with balance, not an absolute rupiah threshold published as a company policy number.",
    ),
    k(
        "2.3.2.2",
        "High Overdue Exposure Count",
        "How many salesmen sit in the top 20% of the team by overdue balance, among reps who have overdue. These books hold a disproportionate share of late money.",
        "This is collection accountability by rep. A salesman can have high piutang that is still current; this KPI is specifically late money.",
        {
            "high": "Overdue is concentrated on several reps’ books.",
            "low": "Few reps dominate overdue.",
        },
        [
            "The same reps remaining in this count.",
            "High overdue exposure with continued strong omzet — they may be selling through the problem.",
        ],
        ["Top Overdue Salesmen", "High Piutang Exposure Count", "Overdue Exposure", "Immediate Collection Count"],
        [
            "Open Top Overdue Salesmen.",
            "Check the customers behind those reps.",
        ],
        [
            "Sales manager and Finance run a joint collection plan on those books.",
            "Consider restricting terms on the worst books.",
        ],
        "Relative top 20% among reps with overdue, not a fixed overdue amount. Alert Center may hide a duplicate of this signal when Collection already raised a high-overdue-workload alert.",
    ),
    k(
        "2.3.2.3",
        "Top Piutang Salesman %",
        "The share of company Total Piutang sitting on the single salesman’s invoiced book with the largest open balance. It is receivable concentration by person.",
        "If one salesman “owns” a large share of what customers owe, that person’s customer quality is a company cash risk. Informational — no automatic threshold.",
        {
            "high": "Working capital is concentrated on one book.",
            "low": "Receivable is spread across more of the force.",
        },
        [
            "Share high and that salesman also Below Target or high overdue.",
            "Share rising while their omzet share is not — they are accumulating debt, not billing.",
        ],
        ["High Piutang Exposure Count", "Top Omzet Salesman %", "Total Piutang"],
        [
            "See the Top 10 Piutang ranking of salesmen.",
            "Check overdue on that book.",
        ],
        [
            "Do not let one book silently become the company’s bank of customer debt.",
        ],
        "Piutang is attributed to the invoicing salesman on the open Faktur, not to a separate collector entity.",
    ),
]

SALESMAN_241 = [
    k(
        "2.4.1.1",
        "Top 10 Salesman (Omzet)",
        "The Sales dashboard ranking of the ten highest invoiced-omzet salesmen. Same commercial idea as the Salesmen dashboard omzet ranking, used in the sales performance room.",
        "It identifies leaders for recognition and for understanding who is carrying company billing.",
        {
            "high": "Read as a ranking of billing leaders.",
            "low": "A thin tail means few producers.",
        },
        [
            "Leaders with weak field execution — results without process.",
            "Leaders missing from achievement ranking — targets may be off, or they are large and still behind plan.",
        ],
        ["Top 10 Omzet Ranking", "Top Omzet", "Top Omzet Salesman %"],
        [
            "Compare with Top 10 Achievement % Ranking.",
            "Check exposure flags on those names.",
        ],
        [
            "Use the list for both recognition and risk concentration.",
        ],
    ),
    k(
        "2.4.1.2",
        "Top Omzet",
        "The field-activity overview ranking of salesmen by invoiced omzet for the selected date context. It is the daily/period producer list next to visits and orders.",
        "It lets owners see whether today’s field work is coming from the same people who win the month, or from different names.",
        {
            "high": "Read as a ranking for the selected field period.",
            "low": "Low top omzet on a selling day is a demand or coverage problem.",
        },
        [
            "Daily top omzet names who are not on the monthly Top 10 — streaky performance.",
            "Daily leaders with poor GPS or execution.",
        ],
        ["Omzet Generated", "Top 10 Salesman (Omzet)", "Top Orders"],
        [
            "Compare with monthly omzet ranking.",
            "Check Effective Call Rate for those names.",
        ],
        [
            "Coach consistency, not only lucky days.",
        ],
    ),
]

SALESMAN_242 = [
    k(
        "2.4.2.1",
        "Top Visit Execution",
        "The salesmen with the highest Visit Execution % in the comparison set. These are the most compliant against their published plans.",
        "Use them as the standard for route discipline — and check that they also sell (Effective Call Rate) so compliance is not empty.",
        {
            "high": "Names at the top completed their plans.",
            "low": "If the “top” is still a modest percentage, the whole team is struggling to finish routes.",
        },
        [
            "Top execution with bottom Effective Call Rate — walking without selling.",
            "Top execution with tiny Planned Visits.",
        ],
        ["Visit Execution %", "Bottom Visit Execution", "Top Effective Call Rate"],
        [
            "Check Planned Visits size.",
            "Check Effective Call Rate for the same names.",
        ],
        [
            "Replicate their planning habits; do not copy volume without checking plan quality.",
        ],
    ),
    k(
        "2.4.2.2",
        "Bottom Visit Execution",
        "The salesmen with the lowest Visit Execution %. These are the coverage-risk names for the day or period.",
        "This is the coaching list for route discipline. Repeat bottom names become dormant-portfolio problems.",
        {
            "high": "Not used as a high-is-bad score; the concern is names at the bottom of the ranking.",
            "low": "Very low execution means the plan was largely not walked.",
        },
        [
            "The same names on Bottom Visit Execution across days.",
            "Bottom execution plus Below Target plus Dormant Portfolio.",
        ],
        ["Visit Execution %", "Missed Visits", "Dormant Portfolio Count", "Below Target Count"],
        [
            "Open Missed Visits for those reps.",
            "Check whether the plan is unrealistic.",
        ],
        [
            "Supervisor intervention the same day or next morning.",
        ],
        "Bottom rankings exist so management does not only look at winners.",
    ),
    k(
        "2.4.2.3",
        "Top Effective Call Rate",
        "The salesmen converting the highest share of visits into same-day orders. These are the most productive callers in the comparison set.",
        "Use them as the conversion standard. Then check whether they also cover the plan — stars who only visit easy customers can look productive and still miss the route.",
        {
            "high": "Top names convert visits to orders.",
            "low": "If even the top rate is weak, the whole team has a conversion problem.",
        },
        [
            "Top rate with low Actual Visits — a tiny, easy day.",
            "Top rate with weak GPS Valid Rate.",
        ],
        ["Effective Call Rate", "Bottom Effective Call Rate", "Top Orders"],
        [
            "Check visit volume and GPS.",
            "Check Orders Generated.",
        ],
        [
            "Spread their call practice; still hold them to the route.",
        ],
    ),
    k(
        "2.4.2.4",
        "Bottom Effective Call Rate",
        "The salesmen converting the lowest share of visits into orders. These are the productivity-coaching names.",
        "They may be highly compliant (high execution) and still commercially ineffective. That combination is a training problem, not a motivation-to-leave-the-office problem.",
        {
            "high": "Read the bottom names, not a single score.",
            "low": "A very low rate means visits are almost never producing orders.",
        },
        [
            "Bottom rate with high missed visits — they are neither covering nor selling.",
            "Bottom rate on reps with large dormant portfolios.",
        ],
        ["Effective Call Rate", "Effective Calls", "Sales Orders", "Dormant Portfolio Count"],
        [
            "Watch a day of calls (replay) and the order list.",
            "Check customer mix — are they visiting non-buyers?",
        ],
        [
            "Coach product, pricing, and ask-for-the-order skills.",
        ],
    ),
]

SALESMAN_243 = [
    k(
        "2.4.3.1",
        "Top Orders",
        "The salesmen who generated the most field orders in the field-activity comparison. This ranking is order documents, not invoiced omzet.",
        "It shows who created demand in the field. Read it next to Top Omzet — orders without invoices, or invoices without orders, are both management questions.",
        {
            "high": "Top names booked many field orders.",
            "low": "If top orders are still few, the force created little pipeline.",
        },
        [
            "Top Orders names missing from Top Omzet — fulfilment or billing lag.",
            "Top Omzet names missing from Top Orders — they may be billing from older pipeline or office-driven invoices.",
        ],
        ["Orders Generated", "Sales Orders", "Top Omzet", "Effective Calls"],
        [
            "Compare with Top Omzet.",
            "Check Effective Call Rate.",
        ],
        [
            "Recognise field order-takers; still verify conversion to Faktur.",
        ],
    ),
]

SALESMAN_244 = [
    k(
        "2.4.4.1",
        "Top Overdue Salesmen",
        "The salesmen whose invoiced books hold the most overdue money. This is the collection ranking by person, from the Collection dashboard.",
        "It answers which reps’ customers are not paying. Use it to assign joint Sales–Finance work, not to blame collectors who are not a separate entity in BTR.",
        {
            "high": "Names at the top carry heavy overdue books.",
            "low": "Even the top overdue book is small — overdue is not person-concentrated.",
        },
        [
            "The same salesmen remaining at the top.",
            "Top overdue salesmen who are also top omzet — selling through late payers.",
        ],
        ["High Overdue Exposure Count", "Overdue Exposure", "Top Overdue Customers", "Immediate Collection Count"],
        [
            "Open the customers behind those salesmen.",
            "Check High Overdue Exposure Count.",
        ],
        [
            "Pair each top overdue salesman with Finance for a named collection list.",
            "Review their credit recommendations.",
        ],
        "Overdue is attributed to the invoicing salesman on the open Faktur. There is no separate Collector master in this KPI.",
    ),
]

SALESMAN_251 = [
    k(
        "2.5.1.1",
        "Active Salesmen",
        "How many salesmen are in the field-activity team view as active for the selected date. It is the size of the force being managed that day — the capacity context for every other field KPI.",
        "Without knowing how many reps were supposed to work, Visit Execution and order totals cannot be interpreted. A weak team day with half the force absent is different from a weak day with everyone out.",
        {
            "high": "A large force was active. Totals should be larger.",
            "low": "A thin force. Do not compare raw visit totals with a full-team day.",
        },
        [
            "Active salesmen dropping over weeks — capacity loss.",
            "Active count high but Planned Visits near zero — they are active without routes.",
        ],
        ["Planned Visits", "Visit Execution %", "Below Target Count", "Missing Target Setup Count"],
        [
            "Check who is missing versus the expected team.",
            "Check planned visits per active salesman.",
        ],
        [
            "Match targets and routes to the real active force.",
            "Investigate unexplained absences operationally — this KPI flags capacity, it does not record HR attendance reasons.",
        ],
        "Active here is the field-overview capacity count. It is not the same rule as Active Customer Count.",
    ),
]

ITEM_311 = [
    k(
        "3.1.1.1",
        "Total Inventory Value",
        "The value of stock on hand at cost (HPP), at the moment of the snapshot. Goods in transit are excluded. This is inventory working capital, not selling-price “shop value”.",
        "It tells the owner how much money is sitting in the warehouse. High is not automatically good or bad — it must be read with health, aging, and days of supply.",
        {
            "high": "More capital is tied in stock. That can support sales or hide dead stock.",
            "low": "Less capital in stock. That can be efficient or a stock-out risk.",
        },
        [
            "Value high while At-Risk Inventory % is also high — the pile is not healthy.",
            "Value falling while Stock-Out Risk Items are rising.",
        ],
        ["Total Item", "At-Risk Inventory %", "Inventory Health Score", "Projected Inventory Value"],
        [
            "Check aging distribution and At-Risk Inventory %.",
            "Check Top Category % — where the capital sits.",
        ],
        [
            "Do not buy more stock from this number alone.",
            "Do not celebrate a low number if service levels are failing.",
        ],
        "Valued at cost (HPP), not selling price. In-transit stock is excluded by business rule.",
    ),
    k(
        "3.1.1.2",
        "Total Item",
        "How many distinct products currently have stock on hand. This is SKU breadth of the live pile, not the size of the product master.",
        "A wide catalogue with low value can mean many tiny leftovers. A narrow catalogue with high value means capital is concentrated in few SKUs.",
        {
            "high": "Many products are sitting in the warehouse.",
            "low": "Few products are in stock — either a tight range or empty shelves.",
        },
        [
            "Item count high and Dead Stock Count also high — breadth is leftover, not assortment strength.",
            "Item count falling while sales still need those SKUs — range is hollowing out.",
        ],
        ["Total Inventory Value", "Dead Stock Count & Value", "Never Sold Count & Value"],
        [
            "Check movement classes.",
            "Check Stock-Out Risk Items for missing active SKUs.",
        ],
        [
            "Trim never-sold and dead SKUs; protect active SKUs that sell.",
        ],
        "This counts products with quantity on hand, not every master-data item with zero stock.",
    ),
]

ITEM_312 = [
    k(
        "3.1.2.1",
        "Top Category %",
        "The share of inventory value sitting in the single largest product category. It is category concentration of capital.",
        "If one category holds most of the warehouse value, a demand shock or aging problem there is a company capital event.",
        {
            "high": "Inventory capital depends heavily on one category.",
            "low": "Capital is spread across more categories.",
        },
        [
            "High share in a category that is also high in Category Risk Exposure.",
            "Share rising without a deliberate stocking strategy.",
        ],
        ["Top 10 Category Ranking", "Top 5 Categories Critical Exposure", "Category Risk Exposure"],
        [
            "Open Top 10 Category Ranking.",
            "Check Category Risk Exposure for that category.",
        ],
        [
            "Purchasing and inventory owners should know why that category is so large.",
        ],
    ),
    k(
        "3.1.2.2",
        "Top 10 Category Ranking",
        "The ten product categories with the most inventory value. This is where warehouse capital actually sits.",
        "Use it for purchasing focus and for aging review — the top categories are where a slow-moving problem becomes expensive.",
        {
            "high": "Read as a ranking of capital by category.",
            "low": "If even the tenth category is small, capital is in a short list of ranges.",
        },
        [
            "A top category also leading Category Risk Exposure.",
            "Categories growing in value while sales for that range are not — overstock forming.",
        ],
        ["Top Category %", "Category Risk Exposure", "Top 5 Categories Critical Exposure"],
        [
            "Cross-read aging for those categories.",
            "Check supplier mix inside the top categories.",
        ],
        [
            "Align replenishment with actual movement, not with last year’s range pride.",
        ],
    ),
    k(
        "3.1.2.3",
        "Top 5 Categories Critical Exposure",
        "The five categories with the largest inventory value, promoted for the executive morning scan. It is a shortened ranking, not a new risk calculation and not an alert row.",
        "Owners use it to ask “where is our stock money?” in one glance, then go to Inventory and Inventory Risk for evidence.",
        {
            "high": "Those five categories are the capital conversation.",
            "low": "Top-five values are modest — capital is less category-concentrated.",
        },
        [
            "The same five categories growing while at-risk % rises.",
        ],
        ["Top 10 Category Ranking", "Top Category %", "At-Risk Inventory %"],
        [
            "Open Inventory Dashboard rankings.",
            "Check Inventory Risk for those categories.",
        ],
        [
            "Morning briefing: name the category owners.",
        ],
        "Informational executive shortlist — not an Alert Center item.",
    ),
]

ITEM_313 = [
    k(
        "3.1.3.1",
        "Category Risk Exposure",
        "How much at-risk inventory value (slow moving, dead, or never sold) sits in each category. It shows where obsolescence capital clusters by range.",
        "A category can be large and healthy, or smaller and sick. This KPI is the sick capital, not the total capital.",
        {
            "high": "That category holds a lot of unhealthy stock money.",
            "low": "Little at-risk value in the category.",
        },
        [
            "A high-value category also showing high risk exposure.",
            "Risk exposure rising while purchasing still replenishes that category.",
        ],
        ["At-Risk Inventory %", "Dead Stock Count & Value", "Slow Moving Count & Value", "Top 10 Category Ranking"],
        [
            "Open Aging Distribution.",
            "Open Top 10 Dead / Slow Moving inside that category.",
        ],
        [
            "Stop or slow replenishment on sick categories; start clearance.",
        ],
    ),
]

ITEM_321 = [
    k(
        "3.2.1.1",
        "Aging Distribution",
        "How inventory value splits across movement classes based on last sale date: Active (sold within the last 89 days), Slow Moving (90–179 days), Dead Stock (180 days or more), and Never Sold (no Faktur history). In-transit is excluded.",
        "This is the health picture of the pile. Totals hide the mix; this distribution is the mix.",
        {
            "high": "Not a single direction. A large Active slice is healthy velocity. Large Slow/Dead/Never Sold slices are capital at risk.",
            "low": "A disappearing Active slice is a warning even if total value still looks fine.",
        },
        [
            "Active shrinking while Dead and Slow grow.",
            "Never Sold remaining material after intake reviews.",
        ],
        ["Dead Stock Count & Value", "Slow Moving Count & Value", "Never Sold Count & Value", "At-Risk Inventory %"],
        [
            "Quantify each unhealthy class.",
            "Check Category Risk Exposure and Supplier Risk Exposure.",
        ],
        [
            "Different actions per class: promote slow movers, clear dead stock, investigate never sold — do not use one tactic for all.",
        ],
        "Movement class uses last Faktur date, not last purchase date. Recently bought but never sold items can already be Never Sold.",
    ),
    k(
        "3.2.1.2",
        "Dead Stock Count & Value",
        "How many items, and how much cost value, have quantity on hand but have not sold for 180 days or more. These are clearance or write-off candidates.",
        "Dead stock is working capital that has already failed commercially. Buying more of it makes the failure larger.",
        {
            "high": "A lot of money and SKUs are frozen in unsold long-idle stock.",
            "low": "Little stock has been allowed to sit half a year without a sale.",
        },
        [
            "Dead value rising while Recommended Purchase Qty still includes related SKUs — optimization should suppress reorder on dead items; if purchasing still buys them, process is leaking.",
            "Dead stock concentrated in one category or supplier.",
        ],
        ["Slow Moving Count & Value", "Top 10 Dead / Slow Moving", "Recoverable Capital", "At-Risk Inventory %"],
        [
            "Open Top 10 Dead / Slow Moving.",
            "Check Recoverable Capital and clearance actions.",
        ],
        [
            "Stop replenishment; start clearance; consider write-off review for unsellable goods.",
        ],
        "Dead Stock is 180 days or more since last sale. It is not the same as Never Sold (no sales history at all).",
    ),
    k(
        "3.2.1.3",
        "Slow Moving Count & Value",
        "How many items, and how much value, have stock but no sale for 90 to 179 days. These SKUs are losing velocity — still salvageable with promotion, bundling, or delayed replenishment.",
        "Slow moving is the warning before dead stock. Acting here is cheaper than clearing after 180 days.",
        {
            "high": "A large band of stock is going quiet. Replenishment should slow down.",
            "low": "Few SKUs are in the 90–179 day idle band.",
        },
        [
            "Slow moving growing into dead stock on the next review.",
            "Slow moving SKUs still appearing in Recommended Purchase Qty.",
        ],
        ["Dead Stock Count & Value", "Aging Distribution", "Action Counts By Type"],
        [
            "List the highest-value slow movers.",
            "Check Delay / Promote actions on Inventory Optimization.",
        ],
        [
            "Delay purchase, promote, or transfer before the 180-day line.",
        ],
        "Slow moving is 90–179 days idle. Do not call it dead stock yet, and do not treat it as active.",
    ),
    k(
        "3.2.1.4",
        "Never Sold Count & Value",
        "How many items, and how much value, have stock on hand but have never sold through a Faktur. This is demand failure or a bad intake decision — not aging of a former seller.",
        "Never Sold is the harshest intake quality signal. It often means the company bought something the market did not want.",
        {
            "high": "Material capital is sitting in products that have never found a customer.",
            "low": "Almost everything in the warehouse has sold at least once.",
        },
        [
            "Never Sold remaining after purchasing reviews.",
            "Never Sold from a principal that is also a compound dependency.",
        ],
        ["Dead Stock Count & Value", "Supplier Risk Exposure", "Recoverable Capital"],
        [
            "See which suppliers and categories produced never-sold intake.",
            "Check Do-Not-Reorder behaviour in optimization actions.",
        ],
        [
            "Return, transfer, or clear; do not reorder.",
            "Review who approved the original purchase.",
        ],
        "Never Sold is not Dead Stock. Dead had a last sale a long time ago. Never Sold never had one.",
    ),
]

ITEM_322 = [
    k(
        "3.2.2.1",
        "At-Risk Inventory %",
        "The share of Total Inventory Value that is slow moving, dead, or never sold. Those three classes do not overlap. This is the percentage of warehouse capital that already needs attention.",
        "Owners use it as the headline inventory-quality ratio. A rising share means the pile is getting sicker even if total value looks stable.",
        {
            "high": "A large fraction of stock money is unhealthy.",
            "low": "Most capital is still in Active stock.",
        },
        [
            "Percentage rising while purchasing is still aggressive.",
            "Percentage high in a top category or top supplier.",
        ],
        ["Aging Distribution", "Inventory Health Score", "Inventory Risk Summary", "Recoverable Capital"],
        [
            "Split into Dead / Slow / Never Sold.",
            "Check Inventory Optimization actions.",
        ],
        [
            "Shift from replenishment to recovery until the share stops rising.",
        ],
        "Copied as context onto Inventory Forecast for traceability. Forecast of active SKUs is a different question from this backward-looking share.",
    ),
    k(
        "3.2.2.2",
        "Inventory Risk Summary",
        "The Alert Center’s rolled-up view of inventory risk — dead, slow, and never sold — presented as a management summary rather than a full SKU dump.",
        "Owners use it on the company-wide exception scan so inventory problems appear next to sales and collection, without opening hundreds of item rows.",
        {
            "high": "The summary is flagging material at-risk stock.",
            "low": "Little inventory risk is being promoted to the alert scan.",
        },
        [
            "Summary quiet while At-Risk Inventory % on Inventory Risk is high — go to the domain dashboard; the alert feed is capped and summarised.",
        ],
        ["At-Risk Inventory %", "Dead Stock Count & Value", "Alert Center inventory alerts"],
        [
            "Open Inventory Risk for the full aging picture.",
            "Then Inventory Optimization for actions.",
        ],
        [
            "Do not try to manage SKUs from Alert Center alone.",
        ],
        "Alert Center summarises item-level risk; it does not list every at-risk SKU.",
    ),
    k(
        "3.2.2.3",
        "Supplier Risk Exposure",
        "How much at-risk inventory value sits with each supplier/principal. It shows which suppliers’ goods are the ones not moving.",
        "A supplier can be a large purchase partner and still be a poor inventory partner. This KPI is the obsolescence lens on supplier mix.",
        {
            "high": "That supplier’s stock is tying up a lot of unhealthy capital.",
            "low": "Little at-risk value from that supplier.",
        },
        [
            "High risk exposure plus ongoing heavy purchase from the same principal.",
            "Overlap with Compound Dependency Count.",
        ],
        ["Category Risk Exposure", "Principal At Risk Count", "Compound Dependency Count", "Top 10 Supplier Ranking"],
        [
            "Open Inventory Risk by supplier.",
            "Check purchasing inactivity vs inventory-no-purchase signals on the Supplier branch.",
        ],
        [
            "Delay or stop buying that principal’s slow lines; clear what is already there.",
        ],
    ),
]

ITEM_323 = [
    k(
        "3.2.3.1",
        "Top 10 Dead / Slow Moving",
        "The highest-impact dead and slow-moving items — the SKU action list by value. This is where clearance and delay decisions should start.",
        "Management cannot work 2,000 slow SKUs at once. This ranking is the practical starting queue.",
        {
            "high": "Items at the top hold a lot of idle capital.",
            "low": "Even the top idle items are small — the problem is a long tail of tiny leftovers, which needs a different (catalogue) clean-up.",
        },
        [
            "The same SKUs remaining on the list.",
            "Top dead SKUs still being purchased.",
        ],
        ["Dead Stock Count & Value", "Slow Moving Count & Value", "Recoverable Capital", "Critical Actions Count"],
        [
            "Take each SKU to Inventory Optimization actions.",
            "Check supplier and category.",
        ],
        [
            "Clear, promote, or stop reorder — one named decision per top SKU.",
        ],
    ),
]

ITEM_331 = [
    k(
        "3.3.1.1",
        "Inventory Health Score",
        "A 0–100 composite of whether active inventory is heading toward stock-out, overstock, or already sitting in at-risk classes. It is a forward-looking health headline for the stock pile, not a financial audit score.",
        "Owners use it like Portfolio Health Score, but for goods. A falling score means replenishment and clearance need attention before service or capital deteriorates further.",
        {
            "high": "Healthier balance of coverage vs excess vs obsolescence.",
            "low": "Stock-out, overstock, or at-risk penalties are dominating.",
        },
        [
            "Score falling while Total Inventory Value looks “normal”.",
            "Score used as a reason to buy more without reading Stock-Out vs Overstock.",
        ],
        ["At-Risk Inventory %", "Stock-Out Risk Items", "Overstock / Understock Value", "Average Days Of Supply"],
        [
            "Split the problem: stock-out list vs overstock vs dead/slow.",
            "Open Inventory Optimization.",
        ],
        [
            "Do not apply one action (always buy, or always freeze buying) from the score alone.",
        ],
        "The optimization dashboard reuses this score; it does not invent a second health metric.",
    ),
]

ITEM_332 = [
    k(
        "3.3.2.1",
        "Projected Inventory Value",
        "What the stock pile is expected to be worth at the end of the 30-day planning horizon if current selling pace continues and nothing else is purchased. Dead and never-sold items are not treated as forecast-eligible consumption.",
        "It is a working-capital projection. If projected value collapses, stock-outs are coming. If it barely moves while sales exist, stock is not turning.",
        {
            "high": "A lot of cost value would still be on hand in 30 days — possible overstock, or slow offtake.",
            "low": "Little value would remain — coverage is thin unless purchasing arrives.",
        },
        [
            "Projected value much lower than today with many Stock-Out Risk Items.",
            "Projected value almost equal to today despite Active sales — idle capital.",
        ],
        ["Scenario Projected Value", "Average Days Of Supply", "Stock-Out Risk Items", "Total Inventory Value"],
        [
            "Check Stock-Out Risk Items and Recommended Purchase Budget.",
            "Check Forecast Confidence.",
        ],
        [
            "Plan replenishment from the stock-out list, not from this total alone.",
        ],
        "Projection uses recent Faktur sales quantity, gross of retur. It is not a promise of future demand.",
    ),
    k(
        "3.3.2.2",
        "Scenario Projected Value",
        "Best / expected / worst projected inventory value using an additional slower/faster consumption band. It shows a range, not a single false-precision number.",
        "Owners should plan against the range. Buying only for the most optimistic scenario is how stock-outs happen; buying for the worst case on every SKU is how dead stock happens.",
        {
            "high": "Even optimistic depletion leaves a large pile — overstock risk.",
            "low": "Even the conservative scenario runs the pile down — coverage risk.",
        },
        [
            "A wide band (best far from worst) — demand is unstable; do not over-commit.",
            "Using only the best case to justify a large purchase.",
        ],
        ["Projected Inventory Value", "Forecast Confidence", "Recommended Purchase Budget"],
        [
            "Read expected first, then worst for service-critical SKUs.",
            "Check Forecast Confidence.",
        ],
        [
            "Size buys inside the range; keep service SKUs safer than slow movers.",
        ],
        "Scenarios are rule-based consumption bands, not AI forecasts.",
    ),
    k(
        "3.3.2.3",
        "Forecast Confidence",
        "How much weight to give this month’s inventory forecast, based on how far the month has run — the same Low / Medium / High pattern used on other forecasts (Low through day 5, Medium days 6–20, High from day 21).",
        "Early-month projections swing. Late-month projections should drive firmer purchase decisions.",
        {
            "high": "Enough of the month has elapsed to steer replenishment more firmly.",
            "low": "Treat projections as directional only.",
        },
        [
            "Placing a large annualised buy on Low confidence.",
            "Ignoring High-confidence stock-out lists.",
        ],
        ["Projected Inventory Value", "Scenario Projected Value", "Recommended Purchase Qty"],
        [
            "If Low, work critical stock-outs only.",
            "If High, execute the optimization queue.",
        ],
        [
            "Match purchase authority to confidence.",
        ],
        "Confidence is calendar progress in the month, not a quality score of the warehouse team.",
    ),
]

ITEM_333 = [
    k(
        "3.3.3.1",
        "Average Days Of Supply",
        "Company-level days of supply: how many days the current on-hand quantity would last at the recent average daily selling pace, across forecast-eligible items. Dead and never-sold items are excluded from this consumption view.",
        "It is the coverage headline. Too few days means service risk; too many means capital is idle. Overstock thinking on this dashboard uses a 90-day supply idea at item level — do not invent a company target beyond what the dashboards already use.",
        {
            "high": "Stock would last a long time at current pace — possible overstock.",
            "low": "Stock would run out soon at current pace — possible stock-out.",
        },
        [
            "Average looking comfortable while Stock-Out Risk Items is high — averages hide holes in the range.",
            "Average very high while At-Risk Inventory % is high — the “days” include goods that are not really selling.",
        ],
        ["Days Of Supply (Item)", "Stock-Out Risk Items", "Overstock / Understock Value"],
        [
            "Open item-level Days Of Supply.",
            "Check stock-out vs overstock lists separately.",
        ],
        [
            "Do not buy to a company average. Buy to holes; delay the excess.",
        ],
        "Company average can look healthy while individual active SKUs are empty. Always read Stock-Out Risk Items.",
    ),
    k(
        "3.3.3.2",
        "Stock-Out Risk Items",
        "How many forecast-eligible items (and their value) are projected to run out within the 30-day horizon at current selling pace. These are the service-risk SKUs.",
        "This is the list that protects sales. Missing these items loses omzet; over-reacting on dead items creates more dead stock.",
        {
            "high": "Many selling SKUs will empty soon if not replenished or transferred.",
            "low": "Few active SKUs are near empty.",
        },
        [
            "Stock-out risk high while Qualified Backlog also high — goods may already have been bought but not posted.",
            "Stock-out SKUs from a principal with Compound Dependency.",
        ],
        ["Recommended Purchase Qty", "Days Of Supply (Item)", "Qualified Backlog Count & Value", "Critical Actions Count"],
        [
            "Check whether posting backlog already covers the supplier.",
            "Check Recommended Purchase Qty and transfer actions.",
        ],
        [
            "Post existing purchases first where the backlog matches.",
            "Then buy, or transfer from a warehouse that is long on the same item.",
        ],
        "Only active, forecast-eligible SKUs. Dead and never-sold items are not “about to stock out” in this model.",
    ),
    k(
        "3.3.3.3",
        "Overstock / Understock Value",
        "The cost value sitting in items that look long on supply versus items that look short, using the forecast’s coverage rules. Overstock thinking includes a 90-day supply threshold at item level; understock is the value attached to coverage holes.",
        "It puts rupiah on the imbalance. A company can be overstocked in total and still understocked on the SKUs that sell.",
        {
            "high": "A large amount of capital is on the wrong SKUs (too much or too little).",
            "low": "Coverage is closer to balanced for forecast-eligible items.",
        },
        [
            "Large overstock value with large understock value at the same time — mix problem, not a total-stock problem.",
        ],
        ["Average Days Of Supply", "Stock-Out Risk Items", "Recoverable Capital", "Recommended Purchase Budget"],
        [
            "Split overstock SKUs (delay/clear) from understock SKUs (buy/transfer).",
            "Check Action Counts By Type.",
        ],
        [
            "Do not freeze all purchasing because overstock value is high — you may still need to fill holes.",
        ],
    ),
]

ITEM_341 = [
    k(
        "3.4.1.1",
        "Days Of Supply (Item)",
        "How many days an individual item’s on-hand quantity would last at its recent average daily sales. This is the SKU-level coverage number behind the company average.",
        "Buy, delay, and transfer decisions are made here, not on the company average. Short DOS on an active SKU is a service risk; long DOS is a capital risk.",
        {
            "high": "That SKU would last a long time — delay or do not reorder.",
            "low": "That SKU will run out soon — replenish, transfer, or post backlog.",
        },
        [
            "Short DOS on best sellers.",
            "Very long DOS on slow movers still in the purchase suggestion list — optimization should delay those.",
        ],
        ["Average Days Of Supply", "Recommended Purchase Qty", "Stock-Out Risk Items"],
        [
            "Sort short DOS active SKUs first.",
            "Check Recommended Purchase Qty.",
        ],
        [
            "One decision per SKU: buy, delay, transfer, or clear.",
        ],
        "Indicative coverage from recent sales pace. It is not a promise, and default lead-time assumptions are planning aids, not supplier contracts.",
    ),
    k(
        "3.4.1.2",
        "Recommended Purchase Qty",
        "An indicative reorder quantity for forecast-eligible items that look short, using recent selling pace plus planning cover. It is a hint for purchasing review, not a purchase order.",
        "Owners should treat it as a starting list. Dead, never sold, and do-not-reorder items should not appear as buys. Qualified backlog may mean “post first” instead of buying more.",
        {
            "high": "The system thinks a lot of units are needed to restore cover.",
            "low": "Little additional quantity is suggested.",
        },
        [
            "Suggestions on slow-moving SKUs — challenge them.",
            "Suggestions while the same supplier has unposted invoices.",
        ],
        ["Recommended Purchase Budget", "Stock-Out Risk Items", "Qualified Backlog Count & Value", "Critical Actions Count"],
        [
            "Filter by Critical Actions.",
            "Check posting backlog for that supplier.",
        ],
        [
            "Purchasing reviews and then orders in Desktop — the portal will not create the purchase.",
        ],
        "Indicative only. Not an automatic PO. Not a commitment.",
    ),
    k(
        "3.4.1.3",
        "Recommended Purchase Budget",
        "The cost value of the indicative purchase suggestions. It answers how much cash replenishment would consume if the hints were followed.",
        "Use it as a cash-planning ceiling discussion, not as an approved spend. Optional budget caps in optimization may defer some lines.",
        {
            "high": "Following the hints would consume substantial purchase cash.",
            "low": "Little indicative spend is on the table.",
        },
        [
            "Budget high while Recoverable Capital is also high — sell or clear before buying the same amount again.",
            "Budget high on Low Forecast Confidence.",
        ],
        ["Recommended Purchase Qty", "Recoverable Capital", "Grand Total Purchase"],
        [
            "Compare with cash and with Recoverable Capital.",
            "Check Action Counts — how much is Purchase vs Delay vs Defer.",
        ],
        [
            "Owner sets a spend appetite; purchasing executes a reviewed subset.",
        ],
        "Not an approved purchase budget in the accounting sense. It is the sum of indicative lines.",
    ),
]

ITEM_342 = [
    k(
        "3.4.2.1",
        "Recoverable Capital",
        "The cost value sitting in dead and slow-moving clearance candidates — money that could come back if those goods are sold, returned, or written down in a controlled way.",
        "This is the recovery prize on the inventory side, parallel to collection impact on the customer side. It is not cash in hand.",
        {
            "high": "A large amount of idle stock could be converted if acted on.",
            "low": "Little clearance value is identified.",
        },
        [
            "Recoverable capital high and no Clearance actions in the queue.",
            "Recoverable capital growing every month.",
        ],
        ["Dead Stock Count & Value", "Slow Moving Count & Value", "Action Counts By Type", "Top 10 Dead / Slow Moving"],
        [
            "Open clearance actions.",
            "Start with Top 10 Dead / Slow Moving.",
        ],
        [
            "Run a clearance programme; stop replenishing those SKUs.",
        ],
        "Recoverable is potential, not collected cash. Write-off is a decision, not a dashboard action.",
    ),
]

ITEM_343 = [
    k(
        "3.4.3.1",
        "Critical Actions Count",
        "How many inventory-optimization actions sit in the most urgent priority band today. These are the first warehouse and purchasing decisions.",
        "Like Actions Today on collection, this is the morning workload for inventory. A high count that is ignored becomes stock-outs and more dead stock.",
        {
            "high": "Many urgent stock decisions are waiting.",
            "low": "Few critical actions — either the pile is calm or the engine has little to say.",
        },
        [
            "Critical actions high while purchasing inactivity is also flagged.",
            "Critical actions repeating on the same SKUs.",
        ],
        ["Action Counts By Type", "Stock-Out Risk Items", "Recommended Purchase Qty"],
        [
            "Split by action type.",
            "Work Post-Purchase-First and stock-out purchases before clearance theatre.",
        ],
        [
            "Inventory and purchasing huddle on the critical list the same day.",
            "The portal will not create POs or transfers.",
        ],
    ),
    k(
        "3.4.3.2",
        "Action Counts By Type",
        "How today’s optimization recommendations split across Purchase, Delay, Transfer, Clearance, and related types (including post-purchase-first and do-not-reorder suppression). It shows what kind of work the pile needs.",
        "If almost all actions are Purchase, the company is chasing holes. If almost all are Clearance/Delay, the company is digesting a past buying mistake. Mix tells the strategy.",
        {
            "high": "Read each type separately. A high Purchase count is not the same story as a high Clearance count.",
            "low": "A type at zero means that lever is not being recommended today.",
        },
        [
            "Purchase counts high while Qualified Backlog is also high — posting may be the real action.",
            "No Delay/Clearance while Dead Stock value is high — recommendations and aging need a joint review.",
        ],
        ["Critical Actions Count", "Recommended Purchase Qty", "Recoverable Capital", "Qualified Backlog Count & Value"],
        [
            "Open each action table.",
            "Match types to owners: Purchasing, Warehouse, Sales (promote).",
        ],
        [
            "Assign each type to a named operator.",
            "Do not execute Purchase recommendations on Do-Not-Reorder goods.",
        ],
        "Recommendations are rule-based and read-only. Precedence exists so do-not-reorder and post-first beat new buying.",
    ),
]

SUPPLIER_411 = [
    k(
        "4.1.1.1",
        "Top Supplier %",
        "The share of inventory value sitting with the single largest supplier. This is warehouse-capital dependence on one principal’s goods.",
        "If one supplier holds most of the stock money, a quality, allocation, or aging problem there is a company event. This is inventory concentration, not this month’s purchase-spend concentration.",
        {
            "high": "Stock capital depends heavily on one supplier.",
            "low": "Stock is spread across more suppliers.",
        },
        [
            "High share plus high Supplier Risk Exposure — dependent and unhealthy.",
            "High inventory share with zero current-month purchase — legacy pile from that supplier.",
        ],
        ["Top 10 Supplier Ranking", "Top 5 Suppliers Critical Exposure", "Supplier Risk Exposure", "Top 1 Principal %"],
        [
            "Open Top 10 Supplier Ranking.",
            "Compare with Top 1 Principal % (purchase spend).",
        ],
        [
            "Do not increase dependence accidentally. Review range and terms with that principal.",
        ],
        "This is inventory-value share, not purchase-invoice share. A supplier can dominate the warehouse without being this month’s biggest buy.",
    ),
    k(
        "4.1.1.2",
        "Top 5 Suppliers Critical Exposure",
        "The five suppliers with the largest inventory value, shown on the executive scan. Shortlist for “whose goods hold our capital?”, not a new calculation.",
        "Owners use it in the morning briefing, then go to Inventory for the ranking and to Inventory Risk for health.",
        {
            "high": "Those five suppliers are the stock-capital conversation.",
            "low": "Top-five values are modest — dependence is weaker.",
        },
        [
            "A top-five supplier also appearing in Principal At Risk Count or Compound Dependency.",
        ],
        ["Top Supplier %", "Top 10 Supplier Ranking", "Compound Dependency Count"],
        [
            "Open Inventory supplier ranking and risk exposure.",
            "Check purchasing concentration for the same names.",
        ],
        [
            "Name an owner for each of the five relationships.",
        ],
        "Executive shortlist — not an Alert Center row.",
    ),
    k(
        "4.1.1.3",
        "Compound Dependency Count",
        "How many principals appear in this month’s top purchase ranking and also in the top inventory ranking or the top at-risk ranking. These suppliers own both the buy and the pile — sometimes the sick pile.",
        "This is multi-dimensional supplier risk. Ordinary concentration says “we buy a lot”. Compound dependency says “we buy a lot and we already hold a lot (or a lot of junk) from them”.",
        {
            "high": "Several principals dominate purchasing and stock (or at-risk stock) at the same time.",
            "low": "Purchase leaders and inventory leaders are more separated.",
        },
        [
            "Any compound dependent principal that is also Principal At Risk.",
            "Count rising while purchasing still increases those lines.",
        ],
        ["Top 1 Principal %", "Top Supplier %", "Supplier Risk Exposure", "Principal At Risk Count"],
        [
            "Name the principals.",
            "Check their at-risk inventory and posting backlog.",
        ],
        [
            "Negotiate, diversify, or freeze additional intake until the pile is healthier.",
        ],
        "A principal can be a top buyer without being compound dependent, if they are not also top inventory or top at-risk.",
    ),
]

SUPPLIER_412 = [
    k(
        "4.1.2.1",
        "Top 1 Principal %",
        "The share of this month’s purchase spend going to the single largest principal. This is buying-concentration — cash flowing out, not stock sitting in.",
        "If one principal takes most of the month’s purchases, allocation, price, or disruption there hits replenishment immediately.",
        {
            "high": "This month’s buying depends heavily on one principal.",
            "low": "Spend is spread across more principals.",
        },
        [
            "Share very high without a deliberate exclusive strategy.",
            "Top 1 principal also compound dependent.",
        ],
        ["Top 3 Principal %", "Top Principal %", "Grand Total Purchase", "Compound Dependency Count"],
        [
            "Open Top 10 Principal Ranking.",
            "Check Top Supplier % to see if the warehouse already matches this dependence.",
        ],
        [
            "Purchasing and owner review whether the dependence is chosen or accidental.",
        ],
    ),
    k(
        "4.1.2.2",
        "Top 3 Principal %",
        "The share of this month’s purchase spend going to the three largest principals combined. It shows whether buying is a three-name market or a broad panel.",
        "A high Top 3 share is still concentrated even if Top 1 looks “reasonable”. Three principals can own the company’s intake.",
        {
            "high": "Almost all buying sits with three names.",
            "low": "Spend is more widely distributed.",
        },
        [
            "Top 3 high and one of the three is at-risk on inventory.",
            "Top 3 rising month after month.",
        ],
        ["Top 1 Principal %", "Top 5 Principals Critical Exposure", "Compound Dependency Count"],
        [
            "See who the three are on Top 10 Principal Ranking.",
            "Check inventory health for those three.",
        ],
        [
            "Build a second source where commercially possible.",
        ],
    ),
    k(
        "4.1.2.3",
        "Top Principal %",
        "The executive headline for purchasing concentration — the leading principal’s share of current-month purchase spend, promoted to the management attention scan.",
        "It is the same dependence question as Top 1 Principal %, used when the owner is not on the Purchasing dashboard.",
        {
            "high": "Intake cash is concentrated.",
            "low": "The leading principal is not dominating spend.",
        },
        [
            "Headline share high plus Qualified Backlog on that principal — you depend on them and their goods are not yet in stock.",
        ],
        ["Top 1 Principal %", "Top 5 Principals Critical Exposure", "Grand Total Purchase"],
        [
            "Open Purchasing Management.",
            "Check Compound Dependency Count.",
        ],
        [
            "Ask Purchasing why that principal owns the month.",
        ],
    ),
    k(
        "4.1.2.4",
        "Top 5 Principals Critical Exposure",
        "The five principals with the largest current-month purchase spend, on the executive shortlist. It is a briefing list of who is receiving company cash for goods.",
        "Pair it with Top 5 Suppliers Critical Exposure (inventory). Spend leaders and stock leaders are not always the same five names — that difference is itself informative.",
        {
            "high": "Those five names are the buying conversation.",
            "low": "Spend is less concentrated in five names.",
        },
        [
            "A top-five principal missing from inventory top five — buying into a thin warehouse position, or goods not yet posted.",
            "A top-five inventory supplier missing from purchase top five — living off old stock.",
        ],
        ["Top 10 Principal Ranking", "Top 5 Suppliers Critical Exposure", "Principal Exposure Comparison"],
        [
            "Compare purchase shortlist with inventory shortlist.",
            "Open Principal Exposure Comparison.",
        ],
        [
            "Morning briefing: cash-out names vs stock-in names.",
        ],
        "Executive shortlist — not an alert row.",
    ),
    k(
        "4.1.2.5",
        "Principal Exposure Comparison",
        "A side-by-side view of principals across purchase spend, inventory value, and at-risk stock. It is the comparison that makes compound dependency visible in names, not only in a count.",
        "Use it when asking “do we buy them, hold them, and is what we hold healthy?” in one look.",
        {
            "high": "Not a single number. Rows where a principal is high on purchase and high on at-risk are the concern.",
            "low": "Principals high on purchase but low on at-risk are healthier dependencies.",
        },
        [
            "High purchase, high inventory, high at-risk on the same row.",
            "High inventory, zero purchase — Principal Inventory No Purchase style signal (legacy stock).",
        ],
        ["Compound Dependency Count", "Top 10 Principal Ranking", "Supplier Risk Exposure", "Principal At Risk Count"],
        [
            "Note names that appear in multiple columns.",
            "Check Qualified Backlog for those names.",
        ],
        [
            "Change intake mix or clear stock before increasing the same exposure.",
        ],
    ),
]

SUPPLIER_413 = [
    k(
        "4.1.3.1",
        "Supplier Risk Exposure",
        "At-risk inventory value by supplier — the same obsolescence-capital idea as on the Item branch, read here as a supplier-dependence risk.",
        "A dependent supplier whose goods are not moving is both a concentration risk and a capital trap.",
        {
            "high": "That supplier’s goods account for a lot of unhealthy stock money.",
            "low": "Little sick stock from that supplier.",
        },
        [
            "Risk exposure high on a Top 1 Principal.",
            "Risk exposure rising after a large buy.",
        ],
        ["Compound Dependency Count", "Principal At Risk Count", "Dead Stock Count & Value", "Top Supplier %"],
        [
            "Open Inventory Risk by supplier.",
            "Check whether purchasing is still replenishing those lines.",
        ],
        [
            "Delay buys; clear stock; talk to the principal about returns or support.",
        ],
    ),
]

SUPPLIER_421 = [
    k(
        "4.2.1.1",
        "Principal At Risk Count",
        "How many principals currently carry a purchasing-management at-risk condition — for example at-risk inventory concentration, compound dependency, or related supplier attention signals on that dashboard.",
        "It is the supplier-level exception count: how many vendor relationships need management review, not how many SKUs are sick.",
        {
            "high": "Many principals are in an attention state. Purchasing leadership is spread thin.",
            "low": "Few principals need exception treatment.",
        },
        [
            "Count rising while Grand Total Purchase is also rising — growing by taking more risk.",
            "A single huge principal inside this count.",
        ],
        ["Compound Dependency Count", "Supplier Risk Exposure", "Top 10 Principal Ranking"],
        [
            "Name the principals on Purchasing Management.",
            "Check inventory and backlog for each.",
        ],
        [
            "Review terms, intake, and clearance per at-risk principal.",
        ],
        "This is a principal count, not an item count. One at-risk principal can represent many SKUs.",
    ),
]

SUPPLIER_422 = [
    k(
        "4.2.2.1",
        "Top 10 Supplier Ranking",
        "The ten suppliers with the most inventory value. This is the warehouse-capital ranking by supplier.",
        "Use it with Top 10 Principal Ranking (spend). Together they show who we hold versus who we are currently paying.",
        {
            "high": "Names at the top hold the stock money.",
            "low": "If the tenth is already small, capital is in few supplier relationships.",
        },
        [
            "Inventory ranking names that do not appear on the purchase ranking this month.",
            "Inventory ranking names with high risk exposure.",
        ],
        ["Top Supplier %", "Top 10 Principal Ranking", "Supplier Risk Exposure"],
        [
            "Compare with purchase ranking.",
            "Check aging for the top inventory suppliers.",
        ],
        [
            "Manage those ten relationships as capital partners, not only as vendors.",
        ],
    ),
]

SUPPLIER_431 = [
    k(
        "4.3.1.1",
        "Grand Total Purchase",
        "The total purchase-invoice value this calendar month. This is cash committed to suppliers for goods, not inventory on hand.",
        "It is the headline purchasing spend. Read it with posting status — bought but unposted is not yet sellable stock.",
        {
            "high": "Heavy intake this month. Check whether it is filling holes or adding to a pile.",
            "low": "Light buying. Check Purchasing Inactivity Flag mid-month and stock-out risk.",
        },
        [
            "Spend high while At-Risk Inventory % is high.",
            "Spend low while Stock-Out Risk Items is high.",
        ],
        ["Total Invoice", "Posted %", "Recommended Purchase Budget", "Top 1 Principal %"],
        [
            "Check Posted % and Qualified Backlog.",
            "Check concentration KPIs.",
        ],
        [
            "Align monthly spend with inventory health, not with habit.",
        ],
        "BTR purchasing is invoice-based; there is no separate Purchase Order entity in this KPI.",
    ),
    k(
        "4.3.1.2",
        "Total Invoice",
        "How many purchase invoices were recorded this month. It is activity volume, not spend size.",
        "Many small invoices or few large ones are different operating loads. Pair the count with Grand Total Purchase.",
        {
            "high": "A busy purchasing document month.",
            "low": "Few invoices — quiet intake or delayed recording.",
        },
        [
            "Invoice count low and spend high — a few large documents; concentration risk.",
            "Invoice count high and Posted % low — processing backlog.",
        ],
        ["Grand Total Purchase", "Posted %", "Pending Posting Value"],
        [
            "Check posting status.",
            "Check inactivity flag if both count and spend are zero mid-month.",
        ],
        [
            "Match admin capacity to invoice volume.",
        ],
    ),
]

SUPPLIER_432 = [
    k(
        "4.3.2.1",
        "Top 10 Principal Ranking",
        "The ten principals with the highest current-month purchase spend. This is who is receiving the company’s buying cash.",
        "It is the working name list for supplier dependence on the spend side.",
        {
            "high": "Names at the top are taking most of the month’s purchases.",
            "low": "Spend is flatter across names.",
        },
        [
            "A new name jumping the list with poor inventory history.",
            "A usual name disappearing — supply break or intentional freeze.",
        ],
        ["Top 1 Principal %", "Principal Exposure Comparison", "Grand Total Purchase"],
        [
            "Compare with Top 10 Supplier Ranking (inventory).",
            "Check Compound Dependency Count.",
        ],
        [
            "Purchasing reviews terms and mix for the top ten every month.",
        ],
    ),
]

SUPPLIER_433 = [
    k(
        "4.3.3.1",
        "Report Footer Grand Total Purchase",
        "The Purchasing Report’s footer total of purchase value. It is the evidence-layer confirmation of Grand Total Purchase, not a second business definition.",
        "Use it when validating the dashboard number before a management argument. If the report is filtered, the footer will not match the unfiltered dashboard.",
        {
            "high": "Same meaning as Grand Total Purchase when unfiltered.",
            "low": "Same meaning as Grand Total Purchase when unfiltered.",
        },
        [
            "Footer and dashboard disagree while filters are on — that is expected. They should agree when the report is unfiltered for the same month.",
        ],
        ["Grand Total Purchase", "Report Footer Total Invoice"],
        [
            "Reconcile unfiltered report to dashboard before acting on a dispute.",
        ],
        [
            "Trust aligned numbers; investigate process if they do not match unfiltered.",
        ],
        "This is the same spend KPI at evidence grain. It is not extra purchasing activity.",
    ),
    k(
        "4.3.3.2",
        "Report Footer Total Invoice",
        "The Purchasing Report’s footer count of invoices. It confirms Total Invoice when the report is unfiltered for the same month.",
        "Use it as traceability, not as a new management question.",
        {
            "high": "Same meaning as Total Invoice when unfiltered.",
            "low": "Same meaning as Total Invoice when unfiltered.",
        },
        [
            "Mismatch with dashboard while search/filter is applied.",
        ],
        ["Total Invoice", "Report Footer Grand Total Purchase"],
        [
            "Clear filters and reconcile.",
        ],
        [
            "Do not manage the company from a filtered footer by accident.",
        ],
        "Evidence-layer confirmation, not a distinct operational KPI.",
    ),
]

SUPPLIER_441 = [
    k(
        "4.4.1.1",
        "Posted %",
        "The share of this month’s purchase value that has already been posted into inventory. Posted means the goods are in stock records; pending means they are still outside sellable inventory from a posting point of view.",
        "Spend that is not posted cannot serve sales. A low posted percentage is an operations delay, not a purchasing-strategy success.",
        {
            "high": "Most of this month’s buys are already in stock records.",
            "low": "A large share of bought value is still waiting to become stock.",
        },
        [
            "Posted % low while Stock-Out Risk Items is high — customers wait while invoices sit unposted.",
            "Posted % low late in the month.",
        ],
        ["Pending Posting Value", "Qualified Backlog Count & Value", "Grand Total Purchase"],
        [
            "Check Pending Posting Value and Qualified Backlog.",
            "See which principals are stuck.",
        ],
        [
            "Warehouse/admin posts aged invoices before new buying of the same goods.",
        ],
        "Posted vs pending is stock-posting status, not supplier payment status.",
    ),
    k(
        "4.4.1.2",
        "Pending Posting Value",
        "The purchase value not yet posted to stock this month. It is the money already committed that is not yet on the shelf in the system.",
        "This is the raw backlog. Qualified Backlog is the aged, actionable slice of similar delay.",
        {
            "high": "A lot of bought goods are still unposted.",
            "low": "Little unposted value.",
        },
        [
            "Pending high and growing day after day.",
            "Pending on principals who also have stock-out SKUs.",
        ],
        ["Posted %", "Qualified Backlog Count & Value", "Stock-Out Risk Items"],
        [
            "Age the pending invoices — see Qualified Backlog.",
            "Prioritise posting for stock-out principals.",
        ],
        [
            "Clear posting before placing duplicate buys.",
        ],
    ),
]

SUPPLIER_442 = [
    k(
        "4.4.2.1",
        "Qualified Backlog Count & Value",
        "How many pending-posting invoices, and how much value, have been waiting at least three calendar days. This is the actionable intake delay — not yesterday’s normal staging.",
        "Qualified backlog is a management concern by design. Optimization will often say “post purchase first” when a stock-out SKU’s supplier already has aged unposted invoices.",
        {
            "high": "Material bought goods have been stuck unposted beyond staging. Sales can be lost while capital is already spent.",
            "low": "Few invoices are aged in pending status.",
        },
        [
            "Backlog high on a compound-dependent principal.",
            "Backlog high while Recommended Purchase Qty still suggests buying the same supplier.",
        ],
        ["Pending Posting Value", "Posted %", "Critical Actions Count", "Stock-Out Risk Items"],
        [
            "List the aged invoices.",
            "Check inventory optimization Post-Purchase-First actions.",
        ],
        [
            "Post first. Do not double-buy.",
            "Fix the warehouse/admin bottleneck that lets invoices age three days.",
        ],
        "The three-calendar-day aging is the approved qualified-backlog rule. Newer unposted invoices are pending, but not yet this KPI.",
    ),
]

SUPPLIER_443 = [
    k(
        "4.4.3.1",
        "Purchasing Inactivity Flag",
        "A signal that no purchase invoices have been recorded this month after a mid-month point. It is a replenishment-gap flag, not a score.",
        "If the warehouse is already long, inactivity can be healthy. If Stock-Out Risk Items are rising, inactivity is a service failure in progress.",
        {
            "high": "The flag is on — intake has gone quiet too late in the month to be “still planning”.",
            "low": "The flag is off — purchasing documents exist, or it is still early.",
        },
        [
            "Flag on plus stock-out risk high.",
            "Flag on while Recommended Purchase Budget is also high — the plan exists, the documents do not.",
        ],
        ["Grand Total Purchase", "Stock-Out Risk Items", "Qualified Backlog Count & Value"],
        [
            "Confirm whether inactivity is a freeze (intentional) or a stall (process failure).",
            "Check stock-out list.",
        ],
        [
            "If unintentional, restart purchasing immediately on critical SKUs.",
            "If intentional, document that it is a freeze so the flag is not a surprise.",
        ],
        "This is a yes/no inactivity flag after a mid-month threshold, not a count of idle days. Exact clock time of “mid-month” beyond that description is a system setting — do not invent a calendar date here.",
    ),
]
