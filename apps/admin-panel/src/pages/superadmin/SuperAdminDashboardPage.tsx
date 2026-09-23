import { useState, useMemo } from 'react';
import { Building2, Users, Package, TrendingUp, Plus, Search, Power, Eye } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { useSuperAdminTenants, useSuperAdminMetrics, useSuperAdminPlanes, useCreateTenant, useUpdateTenantEstado, type CreateTenantRequest } from '@/hooks/useSuperAdmin';
import { toast } from 'sonner';

export function SuperAdminDashboardPage() {
  const { data: tenants, isLoading: tenantsLoading } = useSuperAdminTenants();
  const { data: metrics } = useSuperAdminMetrics();
  const { data: planes } = useSuperAdminPlanes();
  const { mutate: create, isPending: creating } = useCreateTenant();
  const { mutate: updateEstado } = useUpdateTenantEstado();

  const [search, setSearch] = useState('');
  const [open, setOpen] = useState(false);
  const [detail, setDetail] = useState<any | null>(null);
  const [form, setForm] = useState<CreateTenantRequest>({
    emailAdmin: '',
    passwordAdmin: '',
    nombreAdmin: '',
    apellidoAdmin: '',
    nombre: '',
    cuit: '',
    direccion: '',
    logoURL: '',
    telefono: '',
    email: '',
    puntoDeVenta: '',
    condicionVentas: '',
    tipo: 1,
    activo: true,
    idPlan: 1,
    tipoFacturacion: 'Mensual',
    activarSuscripcion: true,
  });

  // sync planes default
  const planesList = planes ?? [];
  const defaultPlanId = planesList[0]?.id ?? 1;

  const filtered = useMemo(() => {
    if (!tenants) return [];
    if (!search) return tenants;
    const q = search.toLowerCase();
    return tenants.filter((t) => t.nombre.toLowerCase().includes(q) || t.cuit.toLowerCase().includes(q) || t.tipo.toLowerCase().includes(q));
  }, [tenants, search]);

  const handleCreate = () => {
    if (!form.nombre.trim() || !form.cuit.trim() || !form.direccion.trim() || !form.emailAdmin.trim() || !form.passwordAdmin.trim() || !form.nombreAdmin.trim() || !form.apellidoAdmin.trim()) {
      toast.error('Completá todos los campos obligatorios (*)');
      return;
    }
    if (!form.emailAdmin.includes('@')) {
      toast.error('Email admin inválido');
      return;
    }
    create({ ...form, idPlan: form.idPlan || defaultPlanId }, {
      onSuccess: () => {
        setOpen(false);
        setForm({
          emailAdmin: '', passwordAdmin: '', nombreAdmin: '', apellidoAdmin: '',
          nombre: '', cuit: '', direccion: '', logoURL: '', telefono: '', email: '', puntoDeVenta: '', condicionVentas: '',
          tipo: 1, activo: true, idPlan: defaultPlanId, tipoFacturacion: 'Mensual', activarSuscripcion: true,
        });
      },
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">Panel Super Admin</h1>
          <p className="text-sm text-muted-foreground">Gestión de negocios y plataforma SaaS</p>
        </div>
        <Button onClick={() => setOpen(true)} className="bg-[#6A4B9F] hover:bg-[#5A3F8A] gap-2">
          <Plus className="h-4 w-4" /> Agregar negocio
        </Button>
      </div>

      {/* Metrics */}
      <div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
        <Card><CardContent className="p-5 flex items-center gap-4"><div className="h-10 w-10 rounded-lg bg-[#6A4B9F] text-white flex items-center justify-center"><Building2 className="h-5 w-5" /></div><div><p className="text-2xl font-bold">{metrics?.totalTenants ?? '-'}</p><p className="text-xs text-muted-foreground">Negocios totales</p></div></CardContent></Card>
        <Card><CardContent className="p-5 flex items-center gap-4"><div className="h-10 w-10 rounded-lg bg-emerald-600 text-white flex items-center justify-center"><TrendingUp className="h-5 w-5" /></div><div><p className="text-2xl font-bold">{metrics?.tenantsActivos ?? '-'}</p><p className="text-xs text-muted-foreground">Activos</p></div></CardContent></Card>
        <Card><CardContent className="p-5 flex items-center gap-4"><div className="h-10 w-10 rounded-lg bg-blue-600 text-white flex items-center justify-center"><Users className="h-5 w-5" /></div><div><p className="text-2xl font-bold">{metrics?.totalUsuarios ?? '-'}</p><p className="text-xs text-muted-foreground">Usuarios</p></div></CardContent></Card>
        <Card><CardContent className="p-5 flex items-center gap-4"><div className="h-10 w-10 rounded-lg bg-amber-600 text-white flex items-center justify-center"><Package className="h-5 w-5" /></div><div><p className="text-2xl font-bold">{metrics?.totalProductos ?? '-'}</p><p className="text-xs text-muted-foreground">Productos</p></div></CardContent></Card>
      </div>

      {/* Search + table */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between gap-4">
          <CardTitle>Negocios contratados</CardTitle>
          <div className="relative w-64">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input placeholder="Buscar por nombre o CUIT..." value={search} onChange={(e) => setSearch(e.target.value)} className="pl-8" />
          </div>
        </CardHeader>
        <CardContent>
          {tenantsLoading ? (
            <p className="text-sm text-muted-foreground">Cargando negocios...</p>
          ) : filtered.length === 0 ? (
            <p className="text-sm text-muted-foreground">No hay negocios. Creá el primero.</p>
          ) : (
            <div className="border rounded-lg overflow-hidden overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-slate-50 border-b">
                  <tr>
                    <th className="text-left p-3 font-medium">Negocio</th>
                    <th className="text-left p-3 font-medium">CUIT</th>
                    <th className="text-left p-3 font-medium">Tipo</th>
                    <th className="text-left p-3 font-medium">Estado</th>
                    <th className="text-left p-3 font-medium">Usuarios</th>
                    <th className="text-left p-3 font-medium">Plan</th>
                    <th className="text-right p-3 font-medium">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {filtered.map((t) => (
                    <tr key={t.id} className="border-b last:border-0 hover:bg-slate-50">
                      <td className="p-3"><div className="font-medium">{t.nombre}</div><div className="text-xs text-muted-foreground">{t.direccion}</div></td>
                      <td className="p-3 font-mono text-xs">{t.cuit}</td>
                      <td className="p-3"><Badge variant="outline">{t.tipo}</Badge></td>
                      <td className="p-3"><Badge variant={t.estado === 'Activo' ? 'default' : 'secondary'} className={t.estado === 'Activo' ? 'bg-emerald-600' : ''}>{t.estado}</Badge></td>
                      <td className="p-3">{t.totalUsuarios} / {t.totalProductos} prod.</td>
                      <td className="p-3 text-xs">{t.suscripcion?.plan ?? 'Sin plan'} <span className="text-muted-foreground">· {t.suscripcion?.estado ?? '-'}</span></td>
                      <td className="p-3 text-right flex justify-end gap-2">
                        <Button variant="outline" size="sm" onClick={() => setDetail(t)}><Eye className="h-3 w-3 mr-1" />Ver</Button>
                        <Button variant="outline" size="sm" onClick={() => {
                          const next = t.estado === 'Activo' ? 'Inactivo' : 'Activo';
                          if (confirm(`¿Cambiar ${t.nombre} a ${next}?`)) updateEstado({ id: t.id, estado: next });
                        }}>
                          <Power className="h-3 w-3 mr-1" />{t.estado === 'Activo' ? 'Suspender' : 'Activar'}
                        </Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Detail dialog */}
      <Dialog open={!!detail} onOpenChange={(o) => !o && setDetail(null)}>
        <DialogContent className="sm:max-w-[520px]">
          <DialogHeader><DialogTitle>Detalle: {detail?.nombre}</DialogTitle></DialogHeader>
          {detail && (
            <div className="space-y-3 text-sm">
              <div className="grid grid-cols-2 gap-3">
                <div><span className="text-muted-foreground">CUIT:</span> {detail.cuit}</div>
                <div><span className="text-muted-foreground">Tipo:</span> {detail.tipo}</div>
                <div><span className="text-muted-foreground">Tel:</span> {detail.telefono ?? '-'}</div>
                <div><span className="text-muted-foreground">Estado:</span> {detail.estado}</div>
              </div>
              <div><span className="text-muted-foreground">Dirección:</span> {detail.direccion}</div>
              <div className="border-t pt-3 grid grid-cols-3 gap-3 text-center">
                <div><p className="text-lg font-bold">{detail.totalUsuarios}</p><p className="text-xs text-muted-foreground">Usuarios</p></div>
                <div><p className="text-lg font-bold">{detail.totalProductos}</p><p className="text-xs text-muted-foreground">Productos</p></div>
                <div><p className="text-lg font-bold">{detail.suscripcion?.plan ?? '-'}</p><p className="text-xs text-muted-foreground">Plan</p></div>
              </div>
              {detail.suscripcion && (
                <div className="bg-slate-50 rounded-lg p-3 text-xs space-y-1">
                  <p><span className="font-medium">Estado suscripción:</span> {detail.suscripcion.estado} · {detail.suscripcion.tipoFacturacion} · ${detail.suscripcion.monto}</p>
                  <p><span className="font-medium">Próximo pago:</span> {detail.suscripcion.fechaProximoPago ? new Date(detail.suscripcion.fechaProximoPago).toLocaleDateString() : '-'}</p>
                </div>
              )}
              <p className="text-xs text-muted-foreground">ID: {detail.id} · Alta: {detail.fechaInicio ? new Date(detail.fechaInicio).toLocaleDateString() : '-'}</p>
            </div>
          )}
          <DialogFooter><Button variant="outline" onClick={() => setDetail(null)}>Cerrar</Button></DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Create dialog */}
      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="sm:max-w-[640px] max-h-[90vh] overflow-y-auto">
          <DialogHeader><DialogTitle>Agregar negocio</DialogTitle></DialogHeader>
          <div className="grid gap-4 py-2">
            <p className="text-xs font-semibold text-[#6A4B9F] uppercase">Datos del administrador (Dueño)</p>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2"><Label>Nombre admin *</Label><Input value={form.nombreAdmin} onChange={(e) => setForm({ ...form, nombreAdmin: e.target.value })} /></div>
              <div className="space-y-2"><Label>Apellido admin *</Label><Input value={form.apellidoAdmin} onChange={(e) => setForm({ ...form, apellidoAdmin: e.target.value })} /></div>
            </div>
            <div className="space-y-2"><Label>Email admin *</Label><Input value={form.emailAdmin} onChange={(e) => setForm({ ...form, emailAdmin: e.target.value })} placeholder="dueño@negocio.com" /></div>
            <div className="space-y-2"><Label>Contraseña admin *</Label><Input type="password" value={form.passwordAdmin} onChange={(e) => setForm({ ...form, passwordAdmin: e.target.value })} placeholder="mín. 6 caracteres" /></div>

            <p className="text-xs font-semibold text-[#6A4B9F] uppercase pt-2 border-t">Datos del negocio</p>
            <div className="space-y-2"><Label>Nombre negocio *</Label><Input value={form.nombre} onChange={(e) => setForm({ ...form, nombre: e.target.value })} /></div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2"><Label>CUIT *</Label><Input value={form.cuit} onChange={(e) => setForm({ ...form, cuit: e.target.value })} placeholder="30-..." /></div>
              <div className="space-y-2"><Label>Tipo</Label>
                <Select value={String(form.tipo)} onValueChange={(v) => { if (v) setForm({ ...form, tipo: Number(v) }); }}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent><SelectItem value="0">Cerrajería</SelectItem><SelectItem value="1">Ferretería</SelectItem><SelectItem value="2">Ambos</SelectItem></SelectContent>
                </Select>
              </div>
            </div>
            <div className="space-y-2"><Label>Dirección *</Label><Input value={form.direccion} onChange={(e) => setForm({ ...form, direccion: e.target.value })} /></div>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2"><Label>Teléfono</Label><Input value={form.telefono ?? ''} onChange={(e) => setForm({ ...form, telefono: e.target.value })} /></div>
              <div className="space-y-2"><Label>Email negocio</Label><Input value={form.email ?? ''} onChange={(e) => setForm({ ...form, email: e.target.value })} /></div>
            </div>

            <p className="text-xs font-semibold text-[#6A4B9F] uppercase pt-2 border-t">Plan y suscripción</p>
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2"><Label>Plan *</Label>
                <Select value={String(form.idPlan)} onValueChange={(v) => { if (v) setForm({ ...form, idPlan: Number(v) }); }}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>{planesList.map((p) => <SelectItem key={p.id} value={String(p.id)}>{p.nombre} — ${p.precioMensual}/mes</SelectItem>)}{planesList.length===0 && <SelectItem value="1">Plan 1 (defecto)</SelectItem>}</SelectContent>
                </Select>
              </div>
              <div className="space-y-2"><Label>Facturación</Label>
                <Select value={form.tipoFacturacion ?? 'Mensual'} onValueChange={(v) => { if (v) setForm({ ...form, tipoFacturacion: v }); }}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent><SelectItem value="Mensual">Mensual</SelectItem><SelectItem value="Anual">Anual</SelectItem></SelectContent>
                </Select>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setOpen(false)}>Cancelar</Button>
            <Button onClick={handleCreate} disabled={creating} className="bg-[#6A4B9F] hover:bg-[#5A3F8A]">{creating ? 'Creando...' : 'Crear negocio'}</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
