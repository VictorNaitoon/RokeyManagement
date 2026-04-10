/**
 * DataTable - Generic data table component with TanStack Table
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 3: UI Components - Reusable data table with sorting, filtering, and pagination
 */

import * as React from 'react';
import {
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getPaginationRowModel,
  getSortedRowModel,
  useReactTable,
} from '@tanstack/react-table';
import type { ColumnDef, SortingState, ColumnFiltersState, VisibilityState, PaginationState } from '@tanstack/react-table';

import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { ChevronDown, ChevronUp, ChevronsUpDown, Search, X } from 'lucide-react';
import { cn } from '@/lib/utils';

export interface DataTableProps<TData, TValue> {
  columns: ColumnDef<TData, TValue>[];
  data: TData[];
  pageCount?: number;
  pageIndex?: number;
  pageSize?: number;
  onPageChange?: (pageIndex: number, pageSize: number) => void;
  onSearch?: (search: string) => void;
  searchPlaceholder?: string;
  searchValue?: string;
  showSearch?: boolean;
  showPagination?: boolean;
  loading?: boolean;
  emptyMessage?: string;
  className?: string;
  onRowClick?: (row: TData) => void;
  getRowClassName?: (row: TData) => string;
}

// Default page sizes
const PAGE_SIZES = [10, 20, 50, 100];

export function DataTable<TData, TValue>({
  columns,
  data,
  pageCount = 0,
  pageIndex = 0,
  pageSize = 20,
  onPageChange,
  onSearch,
  searchPlaceholder = 'Buscar...',
  searchValue = '',
  showSearch = true,
  showPagination = true,
  loading = false,
  emptyMessage = 'No se encontraron datos',
  className,
  onRowClick,
  getRowClassName,
}: DataTableProps<TData, TValue>) {
  // State
  const [sorting, setSorting] = React.useState<SortingState>([]);
  const [columnFilters, setColumnFilters] = React.useState<ColumnFiltersState>([]);
  const [columnVisibility, setColumnVisibility] = React.useState<VisibilityState>({});
  const [rowSelection, setRowSelection] = React.useState({});
  const [pagination, setPagination] = React.useState<PaginationState>({
    pageIndex,
    pageSize,
  });
  const [searchInput, setSearchInput] = React.useState(searchValue);

  // Debounced search
  React.useEffect(() => {
    const timer = setTimeout(() => {
      if (onSearch && searchInput !== searchValue) {
        onSearch(searchInput);
      }
    }, 300);
    return () => clearTimeout(timer);
  }, [searchInput, onSearch, searchValue]);

  // Reset pagination when search changes
  React.useEffect(() => {
    if (searchValue !== undefined && searchValue !== searchInput) {
      setPagination((prev) => ({ ...prev, pageIndex: 0 }));
    }
  }, [searchValue]);

  // Table instance
  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getPaginationRowModel: getPaginationRowModel(),
    getSortedRowModel: getSortedRowModel(),
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onColumnVisibilityChange: setColumnVisibility,
    onRowSelectionChange: setRowSelection,
    onPaginationChange: setPagination,
    pageCount: pageCount > 0 ? pageCount : undefined,
    state: {
      sorting,
      columnFilters,
      columnVisibility,
      rowSelection,
      pagination,
    },
    manualPagination: pageCount > 0,
  });

  // Handle pagination changes
  const handlePageChange = (newPageIndex: number, newPageSize: number) => {
    setPagination({ pageIndex: newPageIndex, pageSize: newPageSize });
    onPageChange?.(newPageIndex, newPageSize);
  };

  return (
    <div className={cn('space-y-4', className)}>
      {/* Search Bar */}
      {showSearch && (
        <div className="flex items-center gap-2">
          <div className="relative flex-1 max-w-sm">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder={searchPlaceholder}
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              className="pl-8"
            />
            {searchInput && (
              <button
                onClick={() => {
                  setSearchInput('');
                  onSearch?.('');
                }}
                className="absolute right-2 top-2.5 text-muted-foreground hover:text-foreground"
              >
                <X className="h-4 w-4" />
              </button>
            )}
          </div>
        </div>
      )}

      {/* Table */}
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((headerGroup) => (
              <TableRow key={headerGroup.id} className="hover:bg-muted/50">
                {headerGroup.headers.map((header) => (
                  <TableHead key={header.id} className={cn(header.column.getCanSort() && 'cursor-pointer select-none')}>
                    {header.isPlaceholder ? null : (
                      <div
                        className={cn(
                          'flex items-center gap-2',
                          header.column.getCanSort() && 'cursor-pointer select-none'
                        )}
                        onClick={header.column.getToggleSortingHandler()}
                      >
                        {flexRender(header.column.columnDef.header, header.getContext())}
                        {header.column.getCanSort() && (
                          <span className="text-muted-foreground">
                            {{
                              asc: <ChevronUp className="h-4 w-4" />,
                              desc: <ChevronDown className="h-4 w-4" />,
                            }[header.column.getIsSorted() as string] ?? <ChevronsUpDown className="h-4 w-4" />}
                          </span>
                        )}
                      </div>
                    )}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={columns.length} className="h-24 text-center text-muted-foreground">
                  Cargando...
                </TableCell>
              </TableRow>
            ) : table.getRowModel().rows?.length ? (
              table.getRowModel().rows.map((row) => (
                <TableRow
                  key={row.id}
                  data-state={row.getIsSelected() && 'selected'}
                  className={cn(
                    onRowClick && 'cursor-pointer',
                    getRowClassName?.(row.original)
                  )}
                  onClick={() => onRowClick?.(row.original)}
                >
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id}>
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : (
              <TableRow>
                <TableCell colSpan={columns.length} className="h-24 text-center text-muted-foreground">
                  {emptyMessage}
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {showPagination && (
        <div className="flex items-center justify-between px-2">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <span>
              Página {table.getState().pagination.pageIndex + 1} de {table.getPageCount()}
            </span>
            {pageCount > 0 && (
              <span className="hidden sm:inline">
                ({data.length} de {pageCount * pageSize} registros)
              </span>
            )}
          </div>
          <div className="flex items-center gap-2">
            {/* Page Size Selector */}
            <Select
              value={String(table.getState().pagination.pageSize)}
              onValueChange={(value) => {
                handlePageChange(0, Number(value));
              }}
            >
              <SelectTrigger className="h-8 w-[70px]">
                <SelectValue />
              </SelectTrigger>
              <SelectContent side="top">
                {PAGE_SIZES.map((size) => (
                  <SelectItem key={size} value={String(size)}>
                    {size}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            {/* Navigation Buttons */}
            <div className="flex items-center gap-1">
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(0, table.getState().pagination.pageSize)}
                disabled={!table.getCanPreviousPage()}
              >
                <ChevronsUpDown className="h-4 w-4 rotate-180" />
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(table.getState().pagination.pageIndex - 1, table.getState().pagination.pageSize)}
                disabled={!table.getCanPreviousPage()}
              >
                <ChevronDown className="h-4 w-4 rotate-180" />
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(table.getState().pagination.pageIndex + 1, table.getState().pagination.pageSize)}
                disabled={!table.getCanNextPage()}
              >
                <ChevronDown className="h-4 w-4" />
              </Button>
              <Button
                variant="outline"
                size="sm"
                onClick={() => handlePageChange(table.getPageCount() - 1, table.getState().pagination.pageSize)}
                disabled={!table.getCanNextPage()}
              >
                <ChevronsUpDown className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default DataTable;