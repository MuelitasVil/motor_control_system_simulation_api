import type { ReactNode } from 'react'

export interface Column<T> {
  label: string
  render: (row: T) => ReactNode
}

export function DataTable<T>({ label, columns, rows, rowKey, empty }: {
  label: string; columns: Column<T>[]; rows: T[]; rowKey: (row: T) => string; empty: string
}) {
  return <div className="table-scroll">
    <table aria-label={label}>
      <thead><tr>{columns.map((column) => <th key={column.label} scope="col">{column.label}</th>)}</tr></thead>
      <tbody>{rows.length ? rows.map((row) =>
        <tr key={rowKey(row)}>{columns.map((column) => <td key={column.label}>{column.render(row)}</td>)}</tr>,
      ) : <tr><td className="empty-table" colSpan={columns.length}>{empty}</td></tr>}</tbody>
    </table>
  </div>
}
