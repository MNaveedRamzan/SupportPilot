interface EmptyStateProps {
  title: string
  description?: string
}

/**
 * Shown when a fetch succeeds but returns no data — distinct from an error,
 * so the user isn't left wondering whether something broke.
 */
export function EmptyState({ title, description }: EmptyStateProps) {
  return (
    <div className="flex flex-col items-center justify-center gap-1 rounded-lg border border-dashed py-10 text-center">
      <p className="text-sm font-medium text-foreground">{title}</p>
      {description && (
        <p className="text-sm text-muted-foreground">{description}</p>
      )}
    </div>
  )
}