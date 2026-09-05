export function formatPeerGroupLabel(args: {
  entityType: string
  peerGroupRuleId?: string | null
  peerGroupSize?: number | null
  dimensionValue?: string | null
}): string | null {
  const size = args.peerGroupSize ?? 0
  if (size <= 0) return null

  const count = String(size)
  const ruleId = args.peerGroupRuleId?.trim().toLowerCase() ?? ''
  const dimension = args.dimensionValue?.trim() || null
  const entityType = args.entityType?.trim().toLowerCase() ?? ''

  if (ruleId === 'customer-klasifikasi') {
    return dimension
      ? `${count} customers in Klasifikasi: ${dimension}`
      : `${count} customers in the same Klasifikasi`
  }

  if (ruleId === 'customer-wilayah') {
    return dimension
      ? `${count} customers in Wilayah: ${dimension}`
      : `${count} customers in the same Wilayah`
  }

  if (ruleId === 'item-principal') {
    return dimension
      ? `${count} items in Principal: ${dimension}`
      : `${count} items in the same Principal`
  }

  if (ruleId === 'item-category') {
    return dimension
      ? `${count} items in Category: ${dimension}`
      : `${count} items in the same category`
  }

  if (ruleId === 'salesman-all-active' || entityType === 'salesman') {
    return `${count} active ${size === 1 ? 'salesman' : 'salesmen'}`
  }

  if (ruleId === 'supplier-all-active' || entityType === 'supplier') {
    return `${count} active ${size === 1 ? 'supplier' : 'suppliers'}`
  }

  // Legacy fallback when rule id is missing: use entity type defaults
  if (!ruleId && entityType === 'customer') {
    return dimension
      ? `${count} customers in Wilayah: ${dimension}`
      : `${count} customers in the same Wilayah`
  }

  if (!ruleId && entityType === 'item') {
    return dimension
      ? `${count} items in Principal: ${dimension}`
      : `${count} items in the same Principal`
  }

  return `${count} ${size === 1 ? 'peer' : 'peers'}`
}

export function peerGroupDimensionHeading(args: {
  peerGroupRuleId?: string | null
  entityType?: string | null
  dimensionValue?: string | null
}): string | null {
  const name = args.dimensionValue?.trim()
  if (!name) return null

  const ruleId = args.peerGroupRuleId?.trim().toLowerCase() ?? ''
  const entityType = args.entityType?.trim().toLowerCase() ?? ''

  if (ruleId === 'customer-klasifikasi') return `Klasifikasi: ${name}`
  if (ruleId === 'customer-wilayah') return `Wilayah: ${name}`
  if (ruleId === 'item-principal') return `Principal: ${name}`
  if (ruleId === 'item-category') return `Category: ${name}`
  if (!ruleId && entityType === 'customer') return `Wilayah: ${name}`
  if (!ruleId && entityType === 'item') return `Principal: ${name}`
  return name
}

export function resolvePeerGroupLabel(args: {
  entityType: string
  distribution: {
    FormattedPeerGroupLabel?: string | null
    PeerGroupRuleId?: string | null
    PeerGroupSize?: number | null
    PeerGroupDimensionValue?: string | null
  }
  populationDimensionValue?: string | null
}): string | null {
  const formatted = args.distribution.FormattedPeerGroupLabel?.trim()
  if (formatted) return formatted

  const dimension =
    args.distribution.PeerGroupDimensionValue?.trim()
    || args.populationDimensionValue?.trim()
    || null

  return formatPeerGroupLabel({
    entityType: args.entityType,
    peerGroupRuleId: args.distribution.PeerGroupRuleId,
    peerGroupSize: args.distribution.PeerGroupSize,
    dimensionValue: dimension,
  })
}
