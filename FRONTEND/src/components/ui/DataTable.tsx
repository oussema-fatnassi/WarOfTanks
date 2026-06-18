import type { ReactNode } from 'react'

export interface Column<T> {
  key: string
  header: ReactNode
  align?: 'left' | 'right' | 'center'
  headerClassName?: string
  cellClassName?: string
  render: (row: T, index: number) => ReactNode
}

interface DataTableProps<T> {
  columns: Column<T>[]
  rows: T[]
  rowKey: (row: T, index: number) => string | number
  /** Per-row <tr> classes; defaults to a hover highlight. */
  rowClassName?: (row: T, index: number) => string
  /** Min width before the table scrolls horizontally, e.g. "min-w-[640px]". */
  minWidth?: string
  footer?: ReactNode
}

const alignClass = {
  left: 'text-left',
  right: 'text-right',
  center: 'text-center',
} as const

/** Generic, presentational table — column defs in, rows rendered. */
function DataTable<T>({
  columns,
  rows,
  rowKey,
  rowClassName,
  minWidth = 'min-w-[640px]',
  footer,
}: DataTableProps<T>) {
  return (
    <div className="overflow-x-auto">
      <table className={`w-full border-collapse ${minWidth}`}>
        <thead>
          <tr className="border-b border-line">
            {columns.map(col => (
              <th
                key={col.key}
                className={`px-5 py-3 font-mono text-[10.5px] font-normal tracking-[1.26px] text-dim uppercase ${alignClass[col.align ?? 'left']} ${col.headerClassName ?? ''}`}
              >
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row, i) => (
            <tr
              key={rowKey(row, i)}
              className={`border-b border-line/60 transition-colors ${rowClassName?.(row, i) ?? 'hover:bg-raised/40'}`}
            >
              {columns.map(col => (
                <td
                  key={col.key}
                  className={`px-5 py-4 align-middle ${alignClass[col.align ?? 'left']} ${col.cellClassName ?? ''}`}
                >
                  {col.render(row, i)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
      {footer}
    </div>
  )
}

export default DataTable
