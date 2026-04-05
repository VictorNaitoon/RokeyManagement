/**
 * KPI Card - Reusable KPI card component
 * RoKey MANAGEMENT - Multi-tenant SaaS ERP/POS for locksmiths
 * 
 * Phase 3: KPI Components
 */

import { type ReactNode } from 'react';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { TrendingUp, TrendingDown } from 'lucide-react';

export interface KPITrend {
  value: number;
  label: string;
}

export interface KPICardProps {
  title: string;
  value: string | number;
  subtitle?: string;
  icon?: ReactNode;
  trend?: KPITrend;
  loading?: boolean;
  variant?: 'default' | 'success' | 'warning' | 'danger';
}

/**
 * Reusable KPI card component with loading state and optional trend indicator
 */
export function KPICard({
  title,
  value,
  subtitle,
  icon,
  trend,
  loading = false,
  variant = 'default',
}: KPICardProps) {
  // Determine accent color based on variant
  const accentColor = {
    default: 'border-l-slate-500',
    success: 'border-l-emerald-500',
    warning: 'border-l-amber-500',
    danger: 'border-l-red-500',
  }[variant];

  // Loading state
  if (loading) {
    return (
      <Card className={`border-l-4 ${accentColor}`}>
        <CardHeader className="pb-2">
          <Skeleton className="h-4 w-24" />
        </CardHeader>
        <CardContent>
          <Skeleton className="h-8 w-32 mb-2" />
          {subtitle && <Skeleton className="h-3 w-20" />}
        </CardContent>
      </Card>
    );
  }

  // Trend indicator
  const isPositiveTrend = trend && trend.value > 0;
  const isNegativeTrend = trend && trend.value < 0;

  return (
    <Card className={`border-l-4 ${accentColor}`}>
      <CardHeader className="pb-2 flex flex-row items-center justify-between space-y-0">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>
        {icon && <div className="text-muted-foreground">{icon}</div>}
      </CardHeader>
      <CardContent>
        <div className="text-2xl font-bold">{value}</div>
        {subtitle && (
          <p className="text-xs text-muted-foreground mt-1">{subtitle}</p>
        )}
        {trend && (
          <div
            className={`flex items-center text-xs mt-2 ${
              isPositiveTrend
                ? 'text-emerald-600'
                : isNegativeTrend
                ? 'text-red-600'
                : 'text-muted-foreground'
            }`}
          >
            {isPositiveTrend ? (
              <TrendingUp className="h-3 w-3 mr-1" />
            ) : isNegativeTrend ? (
              <TrendingDown className="h-3 w-3 mr-1" />
            ) : null}
            <span>
              {isPositiveTrend && '+'}
              {trend.value}% {trend.label}
            </span>
          </div>
        )}
      </CardContent>
    </Card>
  );
}

export default KPICard;