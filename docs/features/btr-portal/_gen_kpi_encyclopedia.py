# -*- coding: utf-8 -*-
"""Generate BTR KPI Encyclopedia HTML. Temporary generator — output is the handbook."""
from pathlib import Path
from html import escape

OUT = Path(__file__).with_name("kpi-encyclopedia.html")


def k(
    code,
    name,
    means,
    matters,
    how,
    warnings,
    related,
    nxt,
    actions,
    misread="No common interpretation issue identified.",
):
    return {
        "code": code,
        "name": name,
        "id": "kpi-" + code.replace(".", "-"),
        "means": means if isinstance(means, list) else [means],
        "matters": matters if isinstance(matters, list) else [matters],
        "how": how,
        "warnings": warnings if isinstance(warnings, list) else [warnings],
        "related": related,
        "next": nxt if isinstance(nxt, list) else [nxt],
        "actions": actions if isinstance(actions, list) else [actions],
        "misread": misread,
    }


# ---------------------------------------------------------------------------
# Customer
# ---------------------------------------------------------------------------

CUSTOMER_111 = [
    k(
        "1.1.1.1",
        "Portfolio Health Score",
        [
            "A single score of how healthy the customer book looks over the next 30 days. It combines how much receivable sits with customers already showing elevated risk, and how many active customers sit in High Risk or Critical.",
            "This is a forward-looking management score, not a credit decision. It does not place a hold, suspend an account, or change plafond.",
        ],
        "Owners use it as the headline for preventive collection and credit planning. A weakening score means problems are forming before they become overdue or dormant.",
        {
            "high": "A higher score generally means a healthier book: fewer customers in serious forward-risk categories and less receivable sitting with them.",
            "low": "A lower score generally means risk is spreading across the book. Collection and credit attention should move earlier, not wait for aging to worsen.",
        },
        [
            "Score moving down from one review to the next.",
            "Score looking healthy while Customers At Risk Count is rising — the average may hide a growing tail.",
            "Score used as if it were an automatic credit stop. It is indicative only.",
        ],
        ["Portfolio Healthy %", "Customers At Risk Count", "High Risk Customer Count", "Elevated Risk Receivable %"],
        [
            "Check Portfolio Healthy % — is the healthy share shrinking?",
            "Check Customers At Risk Count and High Risk Customer Count.",
            "Open Collection Action Queue if the score is falling and overdue is already material.",
        ],
        [
            "Ask Sales and Finance to review the Customer Risk Forecast together.",
            "Do not change credit policy from this score alone — confirm with risk categories and overdue exposure.",
        ],
        "Do not treat this score as profitability, as a credit-limit calculation, or as an automatic hold. Customer value in the portfolio is invoiced omzet, not margin.",
    ),
    k(
        "1.1.1.2",
        "Portfolio Healthy %",
        "The share of customers whose 30-day forecast category is Healthy. It answers: of everyone in the book, how many currently show no forward-risk signals?",
        "It is the executive headline for portfolio quality. It complements older, backward-looking signals such as overdue and dormant, which only appear after the problem has already happened.",
        {
            "high": "A high percentage means most customers have no forecast risk signals. The book is broadly clean.",
            "low": "A low percentage means a large share of customers already carry Watch or worse. Management time will be consumed by protection and collection rather than growth.",
        },
        [
            "Healthy share shrinking while Active Customer Count is stable — quality is deteriorating inside the same book.",
            "Healthy % looking strong while Strategic Customers At Risk Count is rising — the important accounts may be the ones weakening.",
        ],
        ["Portfolio Health Score", "Risk Category - Healthy", "Customers At Risk Count", "Strategic Customers At Risk Count"],
        [
            "Check Risk Category - Watch and Attention — those are the next customers to leave Healthy.",
            "Check Strategic Customers At Risk Count.",
            "Check Customers Forecasted At Risk for the full at-risk population.",
        ],
        [
            "Review Customer Portfolio with Sales leadership.",
            "Protect strategic accounts first if they are leaving the Healthy category.",
        ],
    ),
    k(
        "1.1.1.3",
        "Customers At Risk Count",
        "How many customers sit in Watch, Attention, High Risk, or Critical on the 30-day forecast. This is the breadth of forward risk — how many accounts need preventive attention.",
        "It is an early-warning count. Management uses it to size collection, credit, and sales-recovery workload before overdue or dormancy appears.",
        {
            "high": "More customers are showing forward-risk signals. Preventive work is spreading across a wider book.",
            "low": "Fewer customers need early intervention. Attention can stay concentrated on the remaining cases.",
        },
        [
            "Count rising mid-month while overdue still looks calm — risk is forming ahead of aging.",
            "Count high but Collection Action Queue is light — the forecast is not being converted into daily work.",
        ],
        ["Customers Forecasted At Risk", "High Risk Customer Count", "Risk Category - Watch", "Risk Category - Attention", "Portfolio Health Score"],
        [
            "Split the count using the five Risk Category KPIs.",
            "Check High Risk Customer Count and Risk Category - Critical for severity.",
            "Move to Collection Action Queue for who to contact today.",
        ],
        [
            "Allocate Finance and Sales time to the at-risk list.",
            "Treat this as a planning number, not as an automatic credit freeze.",
        ],
        "This count includes Watch. It is wider than High Risk Customer Count. Do not treat every at-risk customer as already overdue.",
    ),
    k(
        "1.1.1.4",
        "Strategic Customers At Risk Count",
        "How many Strategic-tier customers sit in Watch or worse. These are the highest-importance accounts — material omzet and/or receivable — that are also showing forward risk.",
        "A strategic account in trouble is not the same as a small account in trouble. This KPI tells the owner whether the customers that matter most to the business are the ones weakening.",
        {
            "high": "Important customers are carrying forward risk. Revenue, relationship, and working capital are all exposed together.",
            "low": "Strategic accounts are largely clean. Remaining risk is more likely in smaller or less critical customers.",
        },
        [
            "Any rise in this count — even a few accounts — because the commercial impact is concentrated.",
            "Strategic count rising while overall Customers At Risk looks stable — risk is migrating into the most important names.",
        ],
        ["Strategic Customer Count", "Customers At Risk Count", "Top Omzet Customer %", "Working Capital Tied Amount", "Management Escalation Count"],
        [
            "Identify which strategic customers are at risk in Customer Portfolio.",
            "Check their overdue, plafond, and purchase-decline signals.",
            "If collection is already late, check Management Escalation Count.",
        ],
        [
            "Owner or GM should review the named strategic accounts directly with the sales manager.",
            "Do not apply a generic credit squeeze to the whole book until these accounts are understood.",
        ],
        "Strategic is a computed portfolio tier. It is not the same as Klasifikasi on customer master data.",
    ),
    k(
        "1.1.1.5",
        "High Risk Customer Count",
        "How many customers sit in High Risk or Critical — the severe end of the 30-day forecast. These accounts already combine multiple strong warning signals.",
        "This is the severity cut of the at-risk population. It tells management how many customers need intervention now, not merely monitoring.",
        {
            "high": "Many customers have stacked payment, credit, inactivity, decline, or collection signals. Escalation and credit review will be heavy.",
            "low": "Severe cases are few. Most at-risk customers are still in Watch or Attention.",
        },
        [
            "Count rising while Overdue Customer Count is still modest — the forecast is ahead of aging.",
            "Count concentrated among strategic or top-omzet customers.",
        ],
        ["Risk Category - High Risk", "Risk Category - Critical", "Customers At Risk Count", "Elevated Risk Receivable", "Management Escalation Count"],
        [
            "Split High Risk vs Critical using the category counts.",
            "Check Elevated Risk Receivable to see how much money sits with these customers.",
            "Check Collection Action Queue — Immediate Collection and Management Escalation.",
        ],
        [
            "Prioritize named high-risk accounts with Finance and Sales the same day.",
            "Review credit exposure before releasing further shipments to these customers.",
        ],
        "High Risk Customer Count is narrower than Customers At Risk Count. Watch and Attention are excluded.",
    ),
    k(
        "1.1.1.6",
        "Customers Forecasted At Risk",
        "The Customer Risk Forecast view of customers expected to become a problem within 30 days — overdue, over limit, inactive, or needing early collection. Same population idea as Customers At Risk Count, shown on the forecast dashboard.",
        "Use it when asking “who will become a problem?” rather than “who is already overdue?”. It is the planning list for preventive work.",
        {
            "high": "A large forward-risk population. Collection planning and sales recovery should start before month-end aging deteriorates.",
            "low": "Few customers are forecasted into risk. Monitor Watch-category movement.",
        },
        [
            "Forecasted at-risk rising while current overdue is flat.",
            "Repeated appearance of the same customers across reviews.",
        ],
        ["Customers At Risk Count", "High Risk Customer Count", "Payment Delay Signal Count", "Inactivity Signal Count", "Purchase Decline Signal Count"],
        [
            "Read the signal-family counts to see why they are forecasted at risk.",
            "Then open Collection Action Queue for today’s contact list.",
        ],
        [
            "Run a mid-month risk review with Sales and Finance.",
            "Convert the forecast list into named owners and next actions — the dashboard does not act by itself.",
        ],
        "This is an indicative forecast from business rules, not a prediction of default and not an automatic credit hold.",
    ),
    k(
        "1.1.1.7",
        "Risk Category - Healthy",
        "How many customers have no material 30-day forecast signals — or have no balance and are still purchasing. This is the clean part of the book.",
        "It shows how much of the portfolio can be managed for growth rather than protection. Together with the other four categories it is the risk distribution.",
        {
            "high": "Most customers need only normal monitoring.",
            "low": "The clean core of the book is shrinking. Growth capacity is being replaced by exception work.",
        },
        [
            "Healthy count falling while total customer count is stable.",
            "Healthy looking large while Strategic Customers At Risk is rising.",
        ],
        ["Portfolio Healthy %", "Risk Category - Watch", "Active Customer Count"],
        [
            "If Healthy is shrinking, check Watch next — that is the usual first step out of Healthy.",
            "Then Attention, High Risk, and Critical.",
        ],
        ["Keep commercial focus on Healthy and Watch strategic accounts; do not let collection firefighting consume the whole book."],
    ),
    k(
        "1.1.1.8",
        "Risk Category - Watch",
        "How many customers have early, moderate forward-risk signals but are not yet in Attention. Typical entry is a single moderate signal or two weak signals, with no strong signal.",
        "Watch is the cheapest place to intervene. These customers can often be protected with a reminder, a visit, or a credit check before they become overdue or dormant.",
        {
            "high": "A wide early-warning belt. The team should work this list proactively.",
            "low": "Few early warnings. Either the book is clean, or customers are skipping Watch and landing in worse categories — which is itself a concern.",
        },
        [
            "Watch shrinking while Attention and High Risk are growing — customers are deteriorating, not recovering.",
            "Watch growing rapidly mid-month.",
        ],
        ["Proactive Reminder Count", "Risk Category - Attention", "Customers At Risk Count"],
        [
            "Check Proactive Reminder Count — are Watch customers being contacted before due date?",
            "If Watch is rolling into Attention, check Payment Delay and Credit Limit signals.",
        ],
        [
            "Ask collection to work reminders on Watch accounts with a balance due soon.",
            "Ask sales to visit Watch accounts that also show inactivity or purchase decline.",
        ],
    ),
    k(
        "1.1.1.9",
        "Risk Category - Attention",
        "How many customers have a strong signal, or several moderate signals, but are not yet High Risk. These accounts need a named owner and a specific next step.",
        "Attention is the operating layer between monitoring and crisis. Leaving these customers unowned is how Watch becomes High Risk.",
        {
            "high": "A large intervention queue. Sales and Finance must split the work or both will be late.",
            "low": "Few mid-severity cases. Confirm they are not hiding inside High Risk instead.",
        },
        [
            "Attention count rising together with Due Within 7 Days.",
            "Same customers remaining in Attention across reviews without an action.",
        ],
        ["Credit Review Count", "Sales Recovery Count", "Due Within 7 Days", "Risk Category - High Risk"],
        [
            "Split Attention customers by signal family: payment delay vs credit vs inactivity vs decline.",
            "Route collection cases to the action queue and sales-decline cases to Sales Recovery.",
        ],
        [
            "Assign each Attention customer to Finance or Sales — not both by default, and not neither.",
            "Review credit before expanding supply to Attention accounts with plafond pressure.",
        ],
    ),
    k(
        "1.1.1.10",
        "Risk Category - High Risk",
        "How many customers already combine several strong signals, or mix chronic problems with a new forward signal. These accounts are likely to become overdue, over-limit, or inactive if left alone.",
        "This is the severity list for management intervention. It is the category that should drive credit review and collection priority.",
        {
            "high": "Many accounts need firm collection and credit control now.",
            "low": "Severe cases are contained. Keep them from becoming Critical.",
        },
        [
            "High Risk count rising among top omzet or top piutang customers.",
            "High Risk customers still receiving new sales without a credit review.",
        ],
        ["High Risk Customer Count", "Risk Category - Critical", "Credit Review Count", "Immediate Collection Count", "Elevated Risk Receivable"],
        [
            "List the named High Risk customers and their open balance.",
            "Check Immediate Collection Count and Credit Review Count.",
            "Check Strategic Customers At Risk Count.",
        ],
        [
            "Stop treating these as routine follow-up. Review credit, collection plan, and sales continuation together.",
            "Escalate strategic High Risk accounts to GM/owner.",
        ],
    ),
    k(
        "1.1.1.11",
        "Risk Category - Critical",
        "How many customers sit at the top of the forecast model — several strong signals, or a combination of chronic overdue, plafond pressure, and decline or inactivity. These are the most fragile relationships in the book.",
        "Critical is a management-escalation population. Leaving these accounts to routine collection usually means more aging and more working capital stuck.",
        {
            "high": "The book has a cluster of accounts that can damage both cash and revenue at once.",
            "low": "Few extreme cases. Still review each one individually because impact is rarely small.",
        },
        [
            "Any Critical strategic customer.",
            "Critical count rising while Immediate Collection and Management Escalation queues are empty.",
        ],
        ["Management Escalation Count", "High Risk Customer Count", "Piutang > 90 Hari", "Suspended + Sales Count"],
        [
            "Open Management Escalation Count and the named customer list.",
            "Check Piutang > 90 Hari and Plafond Breach Count for those customers.",
            "Check whether they are still being invoiced.",
        ],
        [
            "Owner/GM should personally review Critical accounts with Sales and Finance.",
            "Decide explicitly: collect and continue, collect and restrict, or exit review. Do not leave the decision implicit.",
        ],
        "Critical is a forecast category, not a legal write-off status. It does not by itself mean the debt is uncollectible.",
    ),
]

CUSTOMER_112 = [
    k(
        "1.1.2.1",
        "Active Customer Count",
        "How many customers were invoiced this month. Active means current billing activity, not that the customer is Healthy on the risk forecast.",
        "It is the simplest measure of a living book. If active count falls, revenue breadth is shrinking even if a few large customers still produce omzet.",
        {
            "high": "More customers are buying this month. Market reach is broad.",
            "low": "Fewer customers are billing. The company may be depending on a smaller core.",
        },
        [
            "Active count falling while Total MTD Omzet is stable — concentration is increasing.",
            "Active count looking healthy while Dormant Count is also rising — the book is rotating, not growing.",
        ],
        ["Dormant Customer Count", "Total MTD Omzet (Portfolio)", "Top Omzet Customer %", "Declining Count"],
        [
            "Check Dormant Customer Count and Declining Count.",
            "Check Top Omzet Customer % — is remaining activity concentrated?",
            "Check Inactivity Signal Count for customers about to leave Active.",
        ],
        [
            "Ask the sales manager why active reach is moving.",
            "Do not celebrate omzet growth if active count is shrinking without a deliberate strategy.",
        ],
        "Active is invoiced this month. A customer can be Active and still Declining or At Risk. Do not read Active as Healthy.",
    ),
    k(
        "1.1.2.2",
        "Dormant Customer Count",
        "How many customers have purchase history but no Faktur for 90 days or more. These accounts used to buy and have gone quiet.",
        "Dormant customers are lost run-rate. They also often still hold open piutang, which becomes legacy debt — inactive accounts that still tie cash.",
        {
            "high": "A large inactive tail. Revenue attrition and possible stuck receivable.",
            "low": "Few silent former buyers. The book is still turning.",
        },
        [
            "Dormant count rising while Active count is flat.",
            "Dormant customers that still have open balance — see Legacy Debt Count.",
        ],
        ["Dormant Count (Lifecycle)", "Never Purchased Count", "Inactivity Signal Count", "Legacy Debt Count", "Dormant Portfolio Count"],
        [
            "Separate Dormant with balance (legacy debt) from Dormant with zero balance (pure attrition).",
            "Check Inactivity Signal Count for customers approaching the 90-day line.",
            "Check which salesmen own the dormant book.",
        ],
        [
            "Ask sales to recover or exit dormant accounts with a named plan.",
            "Ask finance to collect remaining balances on dormant debtors before they age further.",
        ],
        "Dormant requires prior purchase history. Do not confuse with Never Purchased. The 90-day rule is the approved dormant definition.",
    ),
    k(
        "1.1.2.3",
        "Never Purchased Count",
        "How many customer master records have never produced a Faktur. They exist in the book but have never become commercial customers.",
        "This is unused commercial capacity — or dirty master data. Either way it inflates the appearance of a large customer base.",
        {
            "high": "Many registered names have never bought. Coverage looks bigger than it is.",
            "low": "Almost every registered customer has purchased at least once.",
        },
        [
            "Never Purchased remaining large after onboarding campaigns.",
            "Salesmen visiting Never Purchased accounts with no conversion over time — conversion effectiveness is unavailable as a separate KPI in this branch; use this count as the starting flag.",
        ],
        ["Active Customer Count", "Dormant Count (Lifecycle)", "Strategic Customer Count"],
        [
            "Review whether these records are real prospects, duplicates, or inactive master data.",
            "Do not mix them into dormant recovery — they never had a purchase to recover.",
        ],
        [
            "Ask sales which Never Purchased accounts are real targets this month.",
            "Clean or ignore the rest so portfolio percentages are not diluted.",
        ],
        "Never Purchased is not Dormant. Dormant had history and stopped. Never Purchased never started.",
    ),
    k(
        "1.1.2.4",
        "Dormant Count (Lifecycle)",
        "The Customer Portfolio lifecycle view of dormant customers: prior buyers with no Faktur for 90 days or more. Same business idea as Dormant Customer Count, used when deciding grow / retain / recover / exit.",
        "On the portfolio dashboard this count sits next to Never Purchased and Declining so management can see the lifecycle shape of the book, not only this month’s billing.",
        {
            "high": "A large recover-or-exit population.",
            "low": "Few lifecycle-dormant accounts.",
        },
        [
            "Lifecycle dormant rising while Inactivity Signal Count is also rising — more accounts are about to join.",
            "Dormant lifecycle high among strategic customers.",
        ],
        ["Dormant Customer Count", "Declining Count", "Inactivity Signal Count", "Sales Recovery Count"],
        [
            "Check Declining Count — those customers are the usual feeder into dormant.",
            "Check Sales Recovery Count for today’s field recovery work.",
        ],
        [
            "Decide recover vs exit for dormant accounts, especially if they still hold piutang.",
            "Do not keep shipping or extending credit on a dormant relationship without a reset conversation.",
        ],
    ),
    k(
        "1.1.2.5",
        "Declining Count",
        "How many customers are in the Declining lifecycle: purchase trajectory is weakening, based on forecast decline signals. They may still be invoiced this month.",
        "Declining is the last chance to retain a customer before they go dormant. It is a sales problem first, a collection problem second.",
        {
            "high": "Many relationships are cooling. Next period’s Active count is at risk.",
            "low": "Few customers show a weakening purchase path.",
        },
        [
            "Declining count rising among top omzet customers.",
            "Declining overlapping with overdue — the customer is both buying less and paying late.",
        ],
        ["Purchase Decline Signal Count", "Sales Recovery Count", "Active Customer Count", "Dormant Count (Lifecycle)"],
        [
            "Check Purchase Decline Signal Count for the driver.",
            "Check Sales Recovery Count — is the field already tasked to visit them?",
            "Check Top 10 Omzet Ranking to see if important names are in this group.",
        ],
        [
            "Send sales to retain declining accounts before they hit 90 days silent.",
            "Do not wait for Dormant Count to rise — that is after the loss.",
        ],
        "Declining customers can still be Active this month. Current billing does not mean the relationship is stable.",
    ),
    k(
        "1.1.2.6",
        "Inactivity Signal Count",
        "How many customers have a forward inactivity signal: approaching dormant (typically 60–79 days quiet) or imminent dormant (80–89 days), including legacy-forward cases. They are not yet classified dormant.",
        "This is the early warning for dormancy. Acting here is cheaper than recovering a 90-day silent account.",
        {
            "high": "A large group is sliding toward dormant. Sales coverage is missing or ineffective.",
            "low": "Few customers are approaching the 90-day line.",
        },
        [
            "Inactivity signals rising while Planned Visits or Actual Visits are weak on those accounts (investigate on the Salesman branch).",
            "Inactivity plus open balance — tomorrow’s legacy debt.",
        ],
        ["Dormant Customer Count", "Declining Count", "Purchase Decline Signal Count", "Sales Recovery Count"],
        [
            "List customers with inactivity signals and an open balance first.",
            "Check Sales Recovery Count and visit execution for the owning salesman.",
        ],
        [
            "Instruct sales to visit or call these accounts this week.",
            "If they also owe money, coordinate collection so the visit has both a commercial and a cash purpose.",
        ],
        "Inactivity Signal Count is a forecast family, not the same as Dormant Customer Count. Dormant is already past 90 days.",
    ),
    k(
        "1.1.2.7",
        "Purchase Decline Signal Count",
        "How many customers have a purchase-decline forecast signal: moderate drop, severe drop, or stopped buying after having history. This is the driver behind the Declining lifecycle.",
        "It tells management whether the book’s problem is payment, credit, or simply that customers are buying less. Those three problems need different owners.",
        {
            "high": "Demand from existing customers is weakening across many accounts. This is a sales-retention issue.",
            "low": "Few decline signals. If portfolio health is still weak, the driver is more likely payment or credit.",
        },
        [
            "Decline signals concentrated in top omzet customers.",
            "Decline signals with no Sales Recovery Count — the forecast is not reaching the field.",
        ],
        ["Declining Count", "Sales Recovery Count", "Total MTD Omzet (Portfolio)", "Inactivity Signal Count"],
        [
            "Check Declining Count and Sales Recovery Count.",
            "Check whether the same customers also show Payment Delay or Collection Risk — mixed problems need a joint plan.",
        ],
        [
            "Ask the sales manager for a retention plan on declining accounts.",
            "Do not solve a purchase-decline problem only with harder collection.",
        ],
    ),
]

CUSTOMER_121 = [
    k(
        "1.2.1.1",
        "Total Piutang",
        "The total amount customers currently owe on open invoices. It includes balances that are not yet due as well as overdue. This is the size of receivable working capital, not the size of the overdue problem alone.",
        "It is the primary finance exposure number. Every collection, credit, and cash-flow discussion starts from how much of the company’s money sits with customers.",
        {
            "high": "More working capital is tied in customer debt. That can be healthy growth or a collection lag — you cannot know from this number alone.",
            "low": "Less money is outstanding. That can mean strong collection, weaker sales, or both.",
        },
        [
            "Total Piutang rising faster than Total MTD Omzet.",
            "Total Piutang stable but Overdue Exposure rising — quality is worsening inside the same total.",
        ],
        ["Overdue Exposure", "Total Open Balance (Portfolio)", "Piutang > 90 Hari", "Working Capital Tied Amount"],
        [
            "Split current vs overdue using Overdue Exposure.",
            "Check Piutang > 90 Hari for chronic debt.",
            "Check Top 10 Outstanding Customers for concentration.",
        ],
        [
            "Ask Finance for the quality split, not only the total.",
            "Do not restrict sales solely because Total Piutang is high if it is still current and within plafond.",
        ],
        "Total Piutang includes invoices that are not yet due. A high total is not automatically a collection failure. Use Overdue Exposure for urgency.",
    ),
    k(
        "1.2.1.2",
        "Overdue Customer Count",
        "How many customers have any past-due balance. It measures how wide the collection workload is, not how large the overdue money is.",
        "A wide overdue count means many conversations. A small count with large Overdue Exposure means a few heavy debtors. Those are different operating problems.",
        {
            "high": "Collection must cover many accounts. Process and manpower become the constraint.",
            "low": "Few customers are late. Focus on the remaining names and on amounts.",
        },
        [
            "Count rising while Overdue Exposure is flat — more small late accounts.",
            "Count flat while Overdue Exposure rises — fewer, heavier debtors.",
        ],
        ["Overdue Exposure", "Top Overdue Customers", "Payment Delay Signal Count", "Immediate Collection Count"],
        [
            "Check Overdue Exposure for money at stake.",
            "Check Top Overdue Customers for names.",
            "Check Immediate Collection Count for today’s queue.",
        ],
        [
            "Match collection staffing to the count, and collection seniority to the exposure.",
            "Contact the sales manager when overdue customers are also declining or strategic.",
        ],
    ),
    k(
        "1.2.1.3",
        "Piutang > 90 Hari",
        "The amount — and on some dashboards the share — of open receivable that is more than 90 days past due. This is chronic overdue, the bucket associated with bad-debt and escalation risk.",
        "Once debt is this old, routine reminders rarely work. Owners should treat this as a capital-recovery and relationship-decision problem.",
        {
            "high": "A large slice of receivable is chronically late. Write-off, legal, or exit review becomes more likely.",
            "low": "Little debt has been allowed to age this far. Collection is catching issues earlier.",
        },
        [
            "Amount growing month after month.",
            ">90-day share rising even if Total Piutang is stable.",
        ],
        [">90d Exposure", "Top Overdue Customers", "Legacy Debt Count", "Management Escalation Count"],
        [
            "Confirm with >90d Exposure on the Collection view.",
            "Open Top Overdue Customers.",
            "Check Legacy Debt Count if those customers are also dormant.",
        ],
        [
            "Escalate chronic names to GM/owner.",
            "Stop expanding credit to customers who already sit in this bucket.",
        ],
        "Piutang > 90 Hari is days past due date, not days since invoice if the invoice is still within terms. Current (not yet due) debt is not in this KPI.",
    ),
    k(
        "1.2.1.4",
        "Overdue Exposure",
        "The total past-due receivable amount — all overdue aging buckets combined, excluding invoices that are not yet due. This is the monetary size of the collection problem.",
        "Use it when the question is “how much cash is late?”, not “how much do customers owe in total?”. Total Piutang includes current; this number does not.",
        {
            "high": "A large overdue cash gap. Collection must move money, not only close tickets.",
            "low": "Little money is past due. Remaining piutang is largely still within terms.",
        },
        [
            "Overdue Exposure rising while Recovery vs Billing % is below 100% (when omzet is positive).",
            "Overdue Exposure concentrated in a handful of names.",
        ],
        ["Total Piutang", "Overdue Customer Count", "Overdue Concentration %", "Collection Impact Total"],
        [
            "Check Overdue Concentration % and Top Overdue Customers.",
            "Check Due Within 7 Days — next week’s overdue if unpaid.",
            "Check Collection Action Queue.",
        ],
        [
            "Set a collection focus on overdue money, not on total piutang.",
            "Align salesman accountability using Top Overdue Salesmen on the Salesman branch.",
        ],
        "Overdue Exposure is smaller than Total Piutang whenever some invoices are still current. Comparing them as if they were the same number misstates urgency.",
    ),
    k(
        "1.2.1.5",
        ">90d Exposure",
        "The overdue amount that sits only in the more-than-90-days bucket. It is the Collection dashboard’s chronic slice of Overdue Exposure.",
        "It is the escalation and write-off risk in money terms. Together with Piutang > 90 Hari it tells the owner how much capital is already old.",
        {
            "high": "Serious cash is stuck in very late debt.",
            "low": "Chronic overdue is contained.",
        },
        [
            "This amount growing while 1–30 day overdue is also growing — the whole aging ladder is sliding.",
            "Chronic exposure on dormant customers (legacy debt).",
        ],
        ["Piutang > 90 Hari", "Overdue Exposure", "Legacy Debt Count", "Top Overdue Customers"],
        [
            "Name the customers via Top Overdue Customers.",
            "Check Legacy Debt Count.",
            "Check Management Escalation Count.",
        ],
        [
            "Put chronic debtors on an owner-level list.",
            "Decide collect-hard, restructure, or stop supply — do not leave them in the ordinary queue.",
        ],
    ),
    k(
        "1.2.1.6",
        "Overdue Concentration %",
        "The share of total company overdue held by the single largest overdue customer. It answers whether the overdue problem is one name or many.",
        "High concentration means one relationship can move company cash. Collection priority should follow that name, not an even spread of effort.",
        {
            "high": "Overdue cash depends heavily on one customer. A failed collection there is a company event.",
            "low": "Overdue is spread. The problem is process breadth, not a single debtor.",
        },
        [
            "Concentration rising as Overdue Exposure rises — risk is both larger and more concentrated.",
            "The same customer remaining the top overdue name across reviews.",
        ],
        ["Top Overdue Customers", "Top Piutang Customer %", "Overdue Exposure", "Top 5 Customers Critical Exposure"],
        [
            "Open Top Overdue Customers.",
            "Check that customer’s aging, plafond, and strategic tier.",
            "Check Collection Action Queue for that name.",
        ],
        [
            "Owner or Finance lead should personally know the top overdue customer’s plan.",
            "Do not dilute effort across many small overdue accounts while this share is high.",
        ],
        "This is share of overdue, not share of Total Piutang. A customer can dominate overdue without dominating total outstanding.",
    ),
    k(
        "1.2.1.7",
        "Elevated Risk Receivable",
        "How much open receivable sits with customers who are already in an elevated forward-risk category. It translates the risk forecast into money.",
        "Counts tell you how many accounts; this KPI tells you how much cash is attached to them. A few high-balance at-risk customers matter more than many small ones.",
        {
            "high": "A large amount of receivable is already sitting with customers the forecast considers risky.",
            "low": "Most receivable still sits with customers who are not in elevated risk.",
        },
        [
            "Amount rising while Total Piutang is flat — risk is migrating into the existing book.",
            "Amount concentrated in strategic customers.",
        ],
        ["Elevated Risk Receivable %", "Customers At Risk Count", "Total Piutang", "High Risk Customer Count"],
        [
            "Check Elevated Risk Receivable %.",
            "Check High Risk Customer Count and Strategic Customers At Risk Count.",
            "Open Top 10 Outstanding Customers among the at-risk set.",
        ],
        [
            "Tighten credit and collection on the named high-balance at-risk customers.",
            "Do not freeze the whole book if the money is concentrated in a short list.",
        ],
        "This is not a forecast of future sales. It is today’s open balance sitting with customers who have forward-risk signals.",
    ),
    k(
        "1.2.1.8",
        "Elevated Risk Receivable %",
        "The share of Total Piutang that sits with elevated-risk customers. It shows whether risk is a corner of the book or the book itself.",
        "Owners use it to judge how much of working capital is already in the danger zone. A rising share is an early capital-quality warning.",
        {
            "high": "A large fraction of what customers owe is attached to at-risk names.",
            "low": "Most receivable still sits with healthier customers.",
        },
        [
            "Percentage rising even if Total Piutang is unchanged.",
            "Percentage high while Portfolio Healthy % is still being discussed as “fine” — the money and the count can tell different stories.",
        ],
        ["Elevated Risk Receivable", "Portfolio Health Score", "Total Piutang", "Overdue Exposure"],
        [
            "Check Elevated Risk Receivable (amount).",
            "Check whether the share is overdue or still current.",
            "Check Collection Action Queue impact totals.",
        ],
        [
            "Rebalance attention toward the customers who hold that share.",
            "Review credit policy if the share keeps climbing.",
        ],
    ),
    k(
        "1.2.1.9",
        "Total Open Balance (Portfolio)",
        "The sum of open receivable across the Customer Portfolio view. It is the portfolio lens on outstanding balances — the same commercial idea as Total Piutang, shown where management decides grow / protect / collect.",
        "Use it when reading the portfolio dashboard so omzet, open balance, and working capital tied to attention customers stay in one conversation.",
        {
            "high": "The portfolio is carrying a large outstanding book.",
            "low": "Outstanding across the portfolio is small.",
        },
        [
            "Portfolio open balance high while Working Capital Tied Amount (attention customers only) is also high — the problem is in the actionable subset, not only in healthy accounts.",
        ],
        ["Total Piutang", "Working Capital Tied Amount", "Top 10 Piutang Ranking"],
        [
            "Compare with Working Capital Tied Amount (attention customers).",
            "Check Top 10 Piutang Ranking.",
        ],
        [
            "Use portfolio actions (protect, collect, recover, exit review) rather than treating all open balance as one pile.",
        ],
        "This is outstanding receivable, not customer profitability. Portfolio “value” is invoiced omzet, not margin.",
    ),
    k(
        "1.2.1.10",
        "Top 10 Outstanding Customers",
        "The ten customers with the largest open balances, regardless of whether those balances are current or overdue. This is who holds the company’s receivable capital.",
        "Collection and credit conversations should start with names, not only totals. This ranking is the name list for outstanding exposure.",
        {
            "high": "Not directional as a single number. Read the list: if the same names dominate, concentration risk is high.",
            "low": "If balances in the top ten are small relative to Total Piutang, exposure is spread — still review the names.",
        },
        [
            "The same customers occupying the list month after month with growing balances.",
            "Top outstanding customers who are also overdue or at-risk.",
        ],
        ["Top Overdue Customers", "Top Piutang Customer %", "Top 10 Piutang Ranking", "Top 5 Customers Critical Exposure"],
        [
            "Compare with Top Overdue Customers — outstanding is not the same as overdue.",
            "Check plafond and risk category for each name.",
        ],
        [
            "Finance and Sales should have a one-page plan per top outstanding customer.",
            "Owner should know the top few names personally.",
        ],
        "Largest outstanding is not always the largest overdue. A big current customer can sit here while a smaller chronic debtor sits on the overdue list.",
    ),
    k(
        "1.2.1.11",
        "Top Overdue Customers",
        "The customers who hold the most past-due money. This is the collection priority name list.",
        "If Overdue Concentration % is high, this list is short and heavy. If concentration is low, this list is the starting queue, not the whole job.",
        {
            "high": "Read as a ranking, not a score. Large overdue names at the top demand senior collection time.",
            "low": "Smaller overdue names — still collect, but the company-level cash risk is lower.",
        },
        [
            "Names repeating across reviews with little balance reduction.",
            "Top overdue customers who are still being sold.",
        ],
        ["Overdue Concentration %", "Overdue Exposure", "Immediate Collection Count", "Management Escalation Count"],
        [
            "Check Immediate Collection Count for those names.",
            "Check Piutang > 90 Hari on their invoices.",
            "Check whether they are strategic.",
        ],
        [
            "Put the top overdue names on a daily owner list.",
            "Restrict further supply where overdue is chronic and unexplained.",
        ],
    ),
]

CUSTOMER_122 = [
    k(
        "1.2.2.1",
        "Due Within 7 Days",
        "How much currently current receivable will fall due in the next seven days. This is next week’s collection wave if customers pay on time — and next week’s overdue if they do not.",
        "It turns collection from a backward look into a forward plan. Combined with overdue, it is the near-term cash that Finance can still influence.",
        {
            "high": "A large amount comes due immediately. Reminder work this week has high cash leverage.",
            "low": "Little falls due in the next week. Near-term collection leverage is mostly on money already late.",
        },
        [
            "Large due-soon amount sitting with Watch or Attention customers.",
            "Due-soon remaining unpaid and then appearing in Overdue Exposure the following week.",
        ],
        ["Proactive Reminder Count", "Overdue Exposure", "Collection Impact Total", "Planning Confidence"],
        [
            "Check Proactive Reminder Count — is the queue covering these invoices?",
            "Check Payment Delay Signal Count on the same customers.",
        ],
        [
            "Ask collection to run reminders this week on due-soon invoices, especially slow payers.",
            "Do not wait for these invoices to become overdue before calling.",
        ],
        "Due Within 7 Days is not overdue. It is still current. Treating it as already late overstates today’s problem and understates this week’s opportunity.",
    ),
    k(
        "1.2.2.2",
        "Legacy Debt Count",
        "How many customers are dormant (no Faktur for 90 days or more) and still carry an open balance. These are inactive accounts that still owe money.",
        "Legacy debt is low-probability cash with leftover relationship risk. It rarely clears through ordinary monthly collection.",
        {
            "high": "Many silent customers still hold company money. Capital is trapped in dead relationships.",
            "low": "Few inactive debtors. Dormant accounts were either collected or never billed on terms.",
        },
        [
            "Legacy debt rising alongside Dormant Customer Count.",
            "Legacy names also sitting in Piutang > 90 Hari.",
        ],
        ["Dormant Customer Count", "Piutang > 90 Hari", "Credit Review Count", "Management Escalation Count"],
        [
            "List legacy debtors and their aging.",
            "Check whether sales is still assigned to them or the account is abandoned.",
        ],
        [
            "Create a special collection path for legacy debt — not the same as current-customer reminders.",
            "Decide write-off review vs hard collection vs legal, at owner level for material names.",
        ],
        "Legacy debt is dormant plus balance. A dormant customer with zero balance is attrition, not legacy debt.",
    ),
    k(
        "1.2.2.3",
        "Planning Confidence",
        "How much confidence to place in this month’s collection-planning numbers, based on how far the month has already run. Early month is Low, mid month Medium, late month High.",
        "A busy action queue on day 3 of the month is less reliable as a month-end picture than the same queue on day 22. Confidence tells the owner how hard to steer from the plan.",
        {
            "high": "Enough of the month has elapsed that planning figures are more stable.",
            "low": "It is still early. Treat queues and impact totals as directional, not as a month-end forecast.",
        },
        [
            "Using Low-confidence impact totals as if they were committed cash.",
            "Ignoring a High-confidence weak recovery signal.",
        ],
        ["Collection Impact Total", "Recovery vs Billing % (Context)", "Due Within 7 Days"],
        [
            "If confidence is Low, use the queue for today’s work, not for month-end cash calls.",
            "If confidence is High and Recovery vs Billing % is weak, escalate.",
        ],
        [
            "Match decision weight to confidence: light steering early, firm steering late.",
        ],
        "Confidence follows days elapsed in the month (Low at 5 days or fewer, Medium from 6 to 20, High from 21). It is not a judgment of team quality.",
    ),
]

CUSTOMER_123 = [
    k(
        "1.2.3.1",
        "Collection Impact Total",
        "The total collection impact attached to customers who are actionable today — typically overdue plus amounts due within seven days for those accounts. It is the cash that today’s prioritized work is aiming at.",
        "Owners use it to see whether the daily queue is chasing meaningful money or busywork. A long queue with small impact is a staffing design problem.",
        {
            "high": "Today’s prioritized customers carry a lot of near-term collectible exposure.",
            "low": "The actionable list points at relatively little money. Either the book is clean or the queue is missing the heavy names.",
        },
        [
            "Impact high but Actions Today low — work is not covering the money.",
            "Impact low while Overdue Exposure is high — the queue is not aimed at the overdue book.",
        ],
        ["Immediate Impact Total", "Overdue Exposure", "Due Within 7 Days", "Actions Today"],
        [
            "Check Immediate Impact Total for the most urgent slice.",
            "Check Top Overdue Customers against the queue.",
        ],
        [
            "Direct collection time toward the impact list, not only toward easy calls.",
            "The queue is a recommendation, not an automatic Desktop action.",
        ],
        "Impact is a planning amount, not cash already collected. Do not read it as Collection Cash MTD.",
    ),
    k(
        "1.2.3.2",
        "Recovery vs Billing %",
        "How much has been collected this month compared with how much was newly invoiced this month. It answers whether cash recovery is keeping up with new debt creation.",
        "If recovery lags billing, net receivable grows even when sales looks strong. That is a working-capital leak dressed up as growth.",
        {
            "high": "Collections are keeping pace with, or exceeding, new invoicing. The receivable cycle is under control.",
            "low": "New invoices are outrunning collections. Exposure will grow unless billing slows or collection accelerates.",
        },
        [
            "The Collection dashboard treats a ratio below 100% as requiring attention when there is invoiced omzet this month.",
            "Ratio falling while Overdue Exposure is rising.",
        ],
        ["Recovery vs Billing % (Context)", "Total MTD Omzet (Portfolio)", "Overdue Exposure", "Collection Impact Total"],
        [
            "Check Overdue Exposure and Cash collection context on the Collection dashboard.",
            "Check Immediate Collection Count — is the team working the late money?",
        ],
        [
            "If below full recovery against billing, raise collection intensity and review credit on fast-billing slow-paying customers.",
            "Do not celebrate omzet growth without this ratio.",
        ],
        "This uses collections against new invoiced omzet, not against Total Piutang. A company with a large old book can look healthy here while still holding chronic overdue.",
    ),
    k(
        "1.2.3.3",
        "Recovery vs Billing % (Context)",
        "The same Recovery vs Billing story, shown as context on Collection Optimization. It is copied from the Collection dashboard, not recalculated, and can lag the live collection view by a short refresh interval.",
        "It exists so daily collection planners see whether company recovery is keeping up while they work the queue. It is not a second, different ratio.",
        {
            "high": "Same meaning as Recovery vs Billing %: collections are keeping up with new billing.",
            "low": "Same meaning: new debt is outrunning cash-in.",
        },
        [
            "A small difference versus the Collection dashboard because of refresh timing — do not invent a second interpretation.",
        ],
        ["Recovery vs Billing %", "Planning Confidence", "Collection Impact Total"],
        [
            "If the ratio is weak, leave optimization context and review Collection Dashboard overdue and payment mix.",
            "Then return to the action queue.",
        ],
        [
            "Use it as background while assigning today’s calls, not as a separate target.",
        ],
        "Do not treat a copied context KPI as a newly computed result. If numbers differ slightly from Collection, prefer the Collection dashboard as the recovery source.",
    ),
    k(
        "1.2.3.4",
        "Immediate Impact Total",
        "The collection impact sitting on Immediate Collection and Priority Follow-up actions. This is the money attached to the most urgent contact work today.",
        "If Collection Impact Total is large but Immediate Impact is small, the money is in softer actions (reminders, credit review, sales recovery). If Immediate Impact is large, cash is already late or about to be.",
        {
            "high": "Urgent actions point at a lot of money. Today’s collection shift should start here.",
            "low": "Little money sits on the most urgent actions. Other action types may hold more of the plan.",
        },
        [
            "Immediate impact high and Immediate Collection Count low — each call is heavy; use senior collectors.",
            "Immediate impact rising week after week.",
        ],
        ["Collection Impact Total", "Immediate Collection Count", "Overdue Exposure"],
        [
            "Open Immediate Collection Count and the named customers.",
            "Check Top Overdue Customers.",
        ],
        [
            "Clear the immediate queue before spending the day on routine reminders.",
            "Escalate names that repeat on this list.",
        ],
    ),
]

CUSTOMER_131 = [
    k(
        "1.3.1.1",
        "Total MTD Omzet (Portfolio)",
        "Invoiced sales this month across the customer portfolio. This is the company’s current-month billing from customers, used as the portfolio’s value proxy.",
        "It is the growth side of the customer book. Read it together with risk and piutang — omzet without collection is not completed business.",
        {
            "high": "Strong monthly billing from the customer book.",
            "low": "Weak monthly billing. Check active count, declining count, and salesman achievement.",
        },
        [
            "Omzet high while Recovery vs Billing % is weak.",
            "Omzet stable while Active Customer Count falls — concentration.",
        ],
        ["Top Omzet Customer %", "Top 10 Omzet Ranking", "Active Customer Count", "Recovery vs Billing %"],
        [
            "Check Top Omzet Customer % and Top 10 Omzet Ranking.",
            "Check whether omzet sits with at-risk customers.",
        ],
        [
            "Protect the customers who produce this omzet if they appear at risk.",
            "Do not read omzet as profit. Margin is not available in this portfolio view.",
        ],
        "Customer value here is invoiced omzet, not profitability. Gross margin and net sales after retur are not in this KPI.",
    ),
    k(
        "1.3.1.2",
        "Top Omzet Customer %",
        "The share of this month’s invoiced omzet coming from the single largest customer. It is revenue concentration.",
        "If one customer produces a large share of billing, a relationship problem there is a company revenue event. There is no automatic concentration threshold on this KPI — read the share as information.",
        {
            "high": "Monthly revenue depends heavily on one account.",
            "low": "Revenue is spread across more customers.",
        },
        [
            "Share rising while Active Customer Count falls.",
            "The top omzet customer also appearing in risk or overdue lists.",
        ],
        ["Top 10 Omzet Ranking", "Total MTD Omzet (Portfolio)", "Strategic Customer Count", "Top Piutang Customer %"],
        [
            "Open Top 10 Omzet Ranking.",
            "Check that customer’s risk category and overdue.",
        ],
        [
            "Owner should know the top omzet customer personally.",
            "Broaden the book if dependence on one name is becoming the strategy by accident.",
        ],
        "Informational — no automatic warning threshold is defined for this percentage. High is a concentration fact, not a scored alert by itself.",
    ),
    k(
        "1.3.1.3",
        "Top 10 Omzet Ranking",
        "The ten customers with the highest invoiced omzet this month. This is who is producing current revenue.",
        "Management uses it for account-management focus: protect these relationships, and notice when a former top name drops off the list.",
        {
            "high": "Read as a list, not a score. Large names at the top are the revenue core.",
            "low": "If the tenth customer is already small, the revenue core is a short list.",
        },
        [
            "Familiar names disappearing from the list.",
            "Top omzet names also on Top Overdue or High Risk lists.",
        ],
        ["Top Omzet Customer %", "Strategic Customer Count", "Declining Count"],
        [
            "Cross-check each name against risk category and piutang ranking.",
            "Check Declining Count for names still billing but weakening.",
        ],
        [
            "Assign relationship ownership for every top-omzet customer.",
            "Investigate drop-offs as retention issues, not as a curiosity.",
        ],
    ),
]

CUSTOMER_132 = [
    k(
        "1.3.2.1",
        "Top Piutang Customer %",
        "The share of total receivable held by the single largest outstanding customer. This is receivable concentration on the customer analytics view.",
        "If one customer holds a large share of piutang, their payment delay is a company cash event. Like Top Omzet %, this is informational — no automatic threshold is defined on the customer view.",
        {
            "high": "Working capital depends heavily on one debtor.",
            "low": "Receivable is spread across more customers.",
        },
        [
            "Share rising together with Overdue Concentration %.",
            "The same customer dominating both omzet and piutang.",
        ],
        ["Top Customer % (Piutang Concentration)", "Top 10 Piutang Ranking", "Total Piutang", "Overdue Concentration %"],
        [
            "Open Top 10 Piutang Ranking.",
            "Check whether that customer is overdue or still current.",
        ],
        [
            "Review credit and collection on the top piutang customer.",
            "Avoid expanding terms while concentration is already high.",
        ],
        "Informational — no automatic threshold. Do not confuse this with Overdue Concentration %, which is share of overdue only.",
    ),
    k(
        "1.3.2.2",
        "Top Customer % (Piutang Concentration)",
        "The executive and Piutang-dashboard view of how much total piutang sits with the largest customer (and related top-share views). It is the same concentration question as Top Piutang Customer %, used in the morning finance scan.",
        "Owners use it to see default-risk concentration without opening the full customer list. A high share means one relationship can move company cash.",
        {
            "high": "Receivable risk is concentrated.",
            "low": "Receivable is more evenly spread.",
        },
        [
            "Concentration high and the top customer also overdue.",
            "Concentration rising while sales are not — the customer is paying slower, not buying more.",
        ],
        ["Top Piutang Customer %", "Top 5 Customers Critical Exposure", "Top 10 Outstanding Customers"],
        [
            "Open Top 5 Customers Critical Exposure and Top 10 Outstanding Customers.",
            "Check Overdue Concentration %.",
        ],
        [
            "Finance lead reviews the top concentrated names in the daily scan.",
        ],
    ),
    k(
        "1.3.2.3",
        "Top 5 Customers Critical Exposure",
        "The five customers with the largest outstanding balances, promoted for executive priority. It is a short briefing list, not an alert row and not a new calculation.",
        "Owners use it on the morning scan to know which names to ask about first. Detail still lives on the Piutang and Customer dashboards.",
        {
            "high": "Read as a ranking. Large outstanding at the top means those five names are the capital conversation.",
            "low": "Smaller top-five balances — company exposure is less name-concentrated, but still read the list.",
        },
        [
            "The same five names remaining with growing balances.",
            "A top-five customer also appearing as Critical on the risk forecast.",
        ],
        ["Top 10 Outstanding Customers", "Top 10 Piutang Ranking", "Strategic Customers At Risk Count"],
        [
            "Open the domain ranking (Top 10 Outstanding / Top 10 Piutang).",
            "Check risk category and overdue for each name.",
        ],
        [
            "Ask Sales and Finance for a status on each of the five before expanding other topics in the morning meeting.",
        ],
        "This is a shortened ranking for speed. It is not itself an Alert Center item.",
    ),
    k(
        "1.3.2.4",
        "Top 10 Piutang Ranking",
        "The ten customers with the largest open piutang on the customer and portfolio views. It is the working name list for receivable concentration.",
        "Use it to assign account-level capital ownership — who in Sales and Finance is responsible for each heavy balance.",
        {
            "high": "Read as a list. Heavy names at the top are where working capital sits.",
            "low": "If even the top ten are small, receivable is diffuse.",
        },
        [
            "Names on both omzet and piutang top ten with weakening payments.",
            "Piutang ranking names that are dormant or declining.",
        ],
        ["Top 10 Outstanding Customers", "Top Piutang Customer %", "Working Capital Tied Amount"],
        [
            "Compare with Top 10 Omzet Ranking.",
            "Check aging and plafond for each name.",
        ],
        [
            "Give each top-piutang customer a named collector and a named salesman.",
        ],
    ),
]

CUSTOMER_133 = [
    k(
        "1.3.3.1",
        "Strategic Customer Count",
        "How many customers sit in the Strategic portfolio tier — highest importance from omzet, open piutang, purchase frequency, and forward risk. This is a computed tier, not a label copied from customer Klasifikasi.",
        "It tells the owner how large the “must-protect” set is. Too few strategic customers can mean an overly narrow core; the count itself is not a target.",
        {
            "high": "A larger set of accounts is treated as strategically important. Management attention must be real, not ceremonial.",
            "low": "A small strategic set. Those few names are likely critical to omzet or piutang.",
        },
        [
            "Strategic count small and Top Omzet Customer % high — the business is a handful of relationships.",
            "Strategic customers appearing in at-risk counts.",
        ],
        ["Strategic Customers At Risk Count", "Top 10 Omzet Ranking", "Working Capital Tied Amount"],
        [
            "Check Strategic Customers At Risk Count.",
            "Read the named strategic list on Customer Portfolio.",
        ],
        [
            "Owner/GM should have a relationship view of every strategic customer.",
            "Do not use Klasifikasi as a substitute for this tier.",
        ],
        "Strategic is computed. Klasifikasi is only a filter, not the source of this count.",
    ),
    k(
        "1.3.3.2",
        "Working Capital Tied Amount",
        "The open receivable sitting with attention customers — the actionable subset of the portfolio, not the entire book. It is the capital currently tied in customers that already require a management action.",
        "Owners use it to see how much money is stuck in the problem set. Reducing this amount is a capital-release objective, not a sales objective.",
        {
            "high": "A lot of cash is sitting with customers who already need grow/retain/protect/collect/recover/exit decisions.",
            "low": "Little capital is tied in the attention set. Remaining piutang is more likely in healthier accounts.",
        },
        [
            "Tied amount rising while Total Piutang is stable — problems are absorbing more of the book.",
            "Tied amount high and Collection Impact Total low — attention customers are not in the daily collection aim.",
        ],
        ["Total Open Balance (Portfolio)", "Total Piutang", "Collection Impact Total", "Strategic Customer Count"],
        [
            "See which actions those attention customers carry (collect vs recover vs credit review).",
            "Check Strategic Customers inside the attention set.",
        ],
        [
            "Direct collection and credit work at this pool first.",
            "Do not confuse this with company Total Piutang.",
        ],
        "This sums open balance of attention customers only. It is not all working capital in the company, and it is not inventory capital.",
    ),
]

CUSTOMER_141 = [
    k(
        "1.4.1.1",
        "Plafond Breach Count",
        "How many customers currently have an open balance above their approved credit limit (plafond). This is a live credit-policy breach, not a forecast.",
        "Every extra sale to a breached customer is unapproved capital. The count tells the owner whether credit control is being enforced.",
        {
            "high": "Many accounts are over limit. Credit process is leaking.",
            "low": "Few or no current breaches. Limits are being respected, or little credit is in use.",
        },
        [
            "Count above zero on strategic or top-piutang customers.",
            "Breached customers still appearing in new omzet this month.",
        ],
        ["Credit Limit Signal Count", "Credit Review Count", "Suspended + Sales Count", "Total Piutang"],
        [
            "List the breached customers and their excess over limit.",
            "Check Credit Limit Signal Count for projected or approaching breaches.",
            "Check Credit Review Count in the action queue.",
        ],
        [
            "Stop further unapproved supply until Finance reviews each breach.",
            "Ask why the limit was exceeded — sales override, stale plafond, or collection delay.",
        ],
        "Plafond Breach is current position versus limit. Credit Limit Signal Count also includes approaching and projected breaches. They are not the same population.",
    ),
    k(
        "1.4.1.2",
        "Credit Limit Signal Count",
        "How many customers have a forward credit-limit signal: approaching the limit, projected to breach within the horizon, or already breached and worsening. This is the credit-risk family on the 30-day forecast.",
        "It lets management act before a breach, not only after. Projected plafond is an indicative upper bound, not an automatic credit decision.",
        {
            "high": "Many accounts will pressure or break limits soon. Credit review workload is coming.",
            "low": "Few customers are near their limit on the forecast.",
        },
        [
            "Signals rising while Plafond Breach Count is still zero — the problem is forming.",
            "Signals on strategic customers.",
        ],
        ["Plafond Breach Count", "Credit Review Count", "High Risk Customer Count"],
        [
            "Check Plafond Breach Count for already-broken limits.",
            "Check Credit Review Count — is the queue absorbing these signals?",
        ],
        [
            "Review limits and outstanding orders for signalled customers.",
            "Do not treat a projected breach as a completed breach, and do not ignore it either.",
        ],
        "Projected limit use is conservative and indicative. It must not be used as an automatic hold by itself.",
    ),
]

CUSTOMER_142 = [
    k(
        "1.4.2.1",
        "Payment Delay Signal Count",
        "How many customers have a payment-delay forecast signal: likely late payer, escalating overdue, no recent payment, or due-soon slow payer. This is the payment-behaviour family.",
        "It tells management whether the book’s risk is about paying late, as opposed to buying less or exceeding credit. Late payers need collection skill, not only sales visits.",
        {
            "high": "Many customers are signalling slower payment. Overdue Exposure is likely to follow.",
            "low": "Few payment-behaviour warnings.",
        },
        [
            "Delay signals rising while Due Within 7 Days is large.",
            "Delay signals with no Proactive Reminder or Immediate Collection activity.",
        ],
        ["Overdue Customer Count", "Proactive Reminder Count", "Collection Risk Signal Count", "Due Within 7 Days"],
        [
            "Check Due Within 7 Days and Overdue Customer Count.",
            "Check Proactive Reminder Count and Immediate Collection Count.",
        ],
        [
            "Increase reminder and collection intensity on delay-signalled accounts.",
            "Do not wait for them to enter Piutang > 90 Hari.",
        ],
    ),
    k(
        "1.4.2.2",
        "Collection Risk Signal Count",
        "How many customers have a collection-risk forecast signal: due concentration, chronic trajectory, or legacy plus overdue going forward. This family is about collection difficulty, not merely a slow payer.",
        "It flags accounts where ordinary collection is unlikely to be enough — concentrated due dates, chronic paths, or dormant-plus-debt patterns.",
        {
            "high": "Many accounts need specialised collection, not routine reminders.",
            "low": "Few structurally hard collection cases in the forecast.",
        },
        [
            "Collection-risk signals overlapping with Piutang > 90 Hari.",
            "Signals on customers still being sold heavily.",
        ],
        ["Legacy Debt Count", "Piutang > 90 Hari", "Management Escalation Count", "Payment Delay Signal Count"],
        [
            "Check Legacy Debt Count and >90d Exposure.",
            "Check Management Escalation Count.",
        ],
        [
            "Move these accounts off the routine reminder list onto a senior collection path.",
        ],
        "Payment Delay is behaviour. Collection Risk is structural difficulty. A customer can have both.",
    ),
    k(
        "1.4.2.3",
        "Suspended + Sales Count",
        "How many customers are marked suspended on master data and were still invoiced this month. That is a policy violation: the account is supposed to be stopped, but billing continued.",
        "This is a control-failure KPI. Even a small count matters because it means the commercial stop is not being respected.",
        {
            "high": "Suspension is not being enforced. Credit and legal exposure is being created on purpose or by process failure.",
            "low": "Suspended accounts are not being billed — policy is holding.",
        },
        [
            "Any count above zero until each name is explained.",
            "Repeat appearances of the same suspended customer.",
        ],
        ["Plafond Breach Count", "High Risk Customer Count", "Management Escalation Count"],
        [
            "Name the customers and the salesmen who invoiced them.",
            "Check whether suspension is stale master data or a real stop that was overridden.",
        ],
        [
            "Stop further billing until owner/Finance confirms the exception.",
            "Fix the process that allowed the invoice.",
        ],
        "This is not a risk-forecast category. It is a master-data status plus current-month sales — a compliance breach.",
    ),
]

CUSTOMER_151 = [
    k(
        "1.5.1.1",
        "Actions Today",
        "How many customers sit on today’s actionable collection-optimization queue — every action category except defer and no-action. This is the size of today’s contact workload.",
        "It is the morning planning number for Finance and Collection. A queue that is too large will not be finished; a queue that is too small while overdue is high is mis-aimed.",
        {
            "high": "A heavy contact day. Prioritise Immediate Collection and Management Escalation first.",
            "low": "A light actionable set. Confirm that overdue and at-risk customers are not sitting in defer.",
        },
        [
            "Actions Today high and Planning Confidence low — work the list, but do not treat it as month-end truth.",
            "Actions Today low while Overdue Customer Count is high.",
        ],
        ["Immediate Collection Count", "Proactive Reminder Count", "Credit Review Count", "Sales Recovery Count", "Management Escalation Count"],
        [
            "Split the queue using the five action-type counts.",
            "Check Collection Impact Total to see if the workload matches the money.",
        ],
        [
            "Run the morning collection huddle from this queue.",
            "Recommendations are not automatic Desktop tasks — people still have to call and visit.",
        ],
    ),
    k(
        "1.5.1.2",
        "Immediate Collection Count",
        "How many customers are tagged Immediate Collection: overdue with high severity. These are today’s first calls for the collection team.",
        "If this count is ignored, Overdue Exposure does not move. It is the cash-urgent slice of Actions Today.",
        {
            "high": "Many severely overdue customers need contact now.",
            "low": "Few urgent overdue contacts. Other action types may still be busy.",
        },
        [
            "Count rising day after day — collection is not clearing the urgent set.",
            "Count of 1–2 with very large Immediate Impact Total — those names are company-level.",
        ],
        ["Immediate Impact Total", "Top Overdue Customers", "Overdue Exposure", "Actions Today"],
        [
            "Open the named Immediate Collection customers.",
            "Check Immediate Impact Total.",
            "Escalate repeats to Management Escalation.",
        ],
        [
            "Collectors start here before reminders.",
            "Sales should not keep supplying these customers without Finance agreement.",
        ],
    ),
    k(
        "1.5.1.3",
        "Proactive Reminder Count",
        "How many customers should receive a reminder before they are late: they still have a current balance, they are due soon, and they carry forecast risk. This is prevention, not chasing.",
        "Working this list is how Watch customers stay out of Overdue Customer Count. Skipping it makes next week’s Immediate Collection larger.",
        {
            "high": "Many due-soon risky customers. A reminder campaign this week has leverage.",
            "low": "Few preventive reminder cases.",
        },
        [
            "Reminders not worked while Due Within 7 Days is large.",
            "Customers bouncing from Proactive Reminder into Immediate Collection the following week.",
        ],
        ["Due Within 7 Days", "Payment Delay Signal Count", "Risk Category - Watch"],
        [
            "Check Due Within 7 Days.",
            "Check Payment Delay Signal Count.",
        ],
        [
            "Assign reminder calls/WA to collection for this week.",
            "Do not treat reminders as optional if Planning Confidence is already Medium or High.",
        ],
    ),
    k(
        "1.5.1.4",
        "Credit Review Count",
        "How many customers are queued for credit review because of plafond pressure — current breach or projected breach. This queue belongs to Finance, not to collectors chasing overdue.",
        "Credit review prevents new unapproved exposure. Collection without credit review on these names leaves the tap open.",
        {
            "high": "Finance has a large limit-review workload.",
            "low": "Few accounts need a limit decision today.",
        },
        [
            "Credit Review Count low while Plafond Breach Count or Credit Limit Signal Count is high — signals are not reaching the queue.",
        ],
        ["Plafond Breach Count", "Credit Limit Signal Count", "Suspended + Sales Count"],
        [
            "List the credit-review customers and whether they are already breached.",
            "Check whether they are still being invoiced.",
        ],
        [
            "Finance decides continue / restrict / stop before the next shipment.",
            "Do not leave credit review as a label without a decision.",
        ],
    ),
    k(
        "1.5.1.5",
        "Sales Recovery Count",
        "How many customers are queued for a sales recovery visit because purchase decline dominates over collection urgency. These accounts need a salesman, not only a collector.",
        "It is how the system separates “they are not buying” from “they are not paying”. Mixing the two produces the wrong visit.",
        {
            "high": "Many relationships are cooling and need field recovery.",
            "low": "Few decline-dominant cases in today’s queue.",
        },
        [
            "Sales Recovery Count low while Declining Count and Purchase Decline Signal Count are high — the field is not being tasked.",
            "Recovery visits producing no orders and no next date.",
        ],
        ["Declining Count", "Purchase Decline Signal Count", "Inactivity Signal Count", "Dormant Count (Lifecycle)"],
        [
            "Give the named list to the sales manager.",
            "Check visit execution for the owning salesman on the Salesman branch.",
        ],
        [
            "Send sales to retain these customers this week.",
            "Do not send collectors first unless there is also overdue money.",
        ],
        "Sales Recovery is a recommended visit category. It does not itself record whether the visit happened — that is Field Activity on the Salesman branch.",
    ),
    k(
        "1.5.1.6",
        "Management Escalation Count",
        "How many customers require leadership review: critical forecast risk that should not stay in the ordinary collection or sales queue. This is the owner/GM list.",
        "If this count is greater than zero, the daily huddle is incomplete without those names. Escalation exists so serious cases do not hide inside a long action list.",
        {
            "high": "Leadership has several accounts that can move cash and revenue together.",
            "low": "Few or no leadership cases today.",
        },
        [
            "Any strategic or top-piutang customer in this count.",
            "Escalation count rising while no owner decision is recorded in operating practice (the portal itself will not record the decision).",
        ],
        ["Risk Category - Critical", "Strategic Customers At Risk Count", "Piutang > 90 Hari", "Suspended + Sales Count"],
        [
            "Open the named escalation customers.",
            "Review overdue, plafond, and whether sales is continuing.",
        ],
        [
            "Owner/GM reviews these names the same day.",
            "Decide collect, restrict, or exit — then assign Sales and Finance.",
        ],
        "Escalation is a recommendation to review, not a completed management decision.",
    ),
]


if __name__ == "__main__":
    here = Path(__file__).resolve().parent
    ns = {"k": k, "Path": Path, "escape": escape}
    ns.update({n: globals()[n] for n in globals() if n.startswith("CUSTOMER_")})
    exec((here / "_gen_kpi_encyclopedia_rest.py").read_text(encoding="utf-8"), ns)
    exec((here / "_gen_kpi_encyclopedia_html.py").read_text(encoding="utf-8"), ns)
    html = ns["render_document"](ns)
    out = here / "kpi-encyclopedia.html"
    out.write_text(html, encoding="utf-8")
    keys = [x for x in ns if x[:3] in ("CUS", "SAL", "ITE", "SUP") and isinstance(ns.get(x), list)]
    n = sum(len(ns[x]) for x in keys)
    print(f"Wrote {out} ({out.stat().st_size:,} bytes, {n} KPI cards)")
