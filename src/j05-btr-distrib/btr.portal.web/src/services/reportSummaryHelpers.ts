import type {
  InventoryReportRow,
  PiutangReportRow,
  PurchasingReportRow,
} from '@/models/reports'

export function summarizePiutangRows(rows: PiutangReportRow[]) {
  const customerKeys = rows
    .map((row) => row.CustomerName.trim())
    .filter((name) => name.length > 0)

  return {
    TotalPiutang: rows.reduce((sum, row) => sum + row.KurangBayar, 0),
    TotalCustomer: new Set(customerKeys.map((name) => name.toLowerCase())).size,
  }
}

export function summarizePurchasingRows(rows: PurchasingReportRow[]) {
  return {
    GrandTotalPurchase: rows.reduce((sum, row) => sum + row.GrandTotal, 0),
    TotalInvoice: rows.length,
  }
}

export function summarizeInventoryRows(rows: InventoryReportRow[]) {
  const totalsByItem = new Map<string, { qty: number; hpp: number }>()

  for (const row of rows) {
    const existing = totalsByItem.get(row.BrgId)
    if (existing) {
      existing.qty += row.Qty
    } else {
      totalsByItem.set(row.BrgId, { qty: row.Qty, hpp: row.Hpp })
    }
  }

  let totalInventoryValue = 0
  let totalItem = 0

  for (const item of totalsByItem.values()) {
    if (item.qty > 0) {
      totalItem += 1
      totalInventoryValue += item.hpp * item.qty
    }
  }

  return {
    TotalInventoryValue: totalInventoryValue,
    TotalItem: totalItem,
  }
}
