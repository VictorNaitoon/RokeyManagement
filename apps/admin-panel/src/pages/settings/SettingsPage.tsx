import { useState, useEffect } from 'react';
import { authStore } from '@/stores/authStore';
import { useNegocio, useUpdateNegocio } from '@/hooks/useNegocio';
import { useUsuarios, useCreateUsuario, useUpdateUsuario, useDeleteUsuario, useCambiarPassword, useUpdatePerfil } from '@/hooks/useUsuarios';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogFooter } from '@/components/ui/dialog';
import { toast } from 'sonner';

type Tab = 'perfil' | 'negocio' | 'usuarios';

const tipoToInt: Record<string, number> = {
  Cerrajeria: 0,
  Ferreteria: 1,
  Ambos: 2,
  MotoRepuestos: 2,
  AutoRepuestos: 3,
};

const intToTipoLabel: Record<number, string> = {
  0: 'Cerrajería',
  1: 'Ferretería',
  2: 'Ambos',
};

export function SettingsPage() {
  const user = authStore((s) => s.user);
  const rol = user?.rol ?? 'Empleado';
  const isDueño = rol === 'Dueño';
  const isGerente = rol === 'Gerente';

  const [tab, setTab] = useState<Tab>('perfil');

  // Auto-select first allowed tab if current is not allowed
  useEffect(() => {
    if (tab === 'usuarios' && !isDueño) setTab('perfil');
    if (tab === 'negocio' && rol === 'Empleado') setTab('perfil');
  }, [rol, tab, isDueño]);

  return (
    <div className="space-y-6 max-w-5xl">
      <div>
        <h1 className="text-2xl font-bold">Configuración</h1>
        <p className="text-sm text-muted-foreground">Gestioná tu perfil y la información del negocio</p>
      </div>

      {/* Tabs */}
      <div className="flex gap-2 border-b pb-2 flex-wrap">
        <TabButton active={tab === 'perfil'} onClick={() => setTab('perfil')}>Mi Perfil</TabButton>
        {(isDueño || isGerente) && (
          <TabButton active={tab === 'negocio'} onClick={() => setTab('negocio')}>Mi Negocio</TabButton>
        )}
        {isDueño && (
          <TabButton active={tab === 'usuarios'} onClick={() => setTab('usuarios')}>Usuarios</TabButton>
        )}
      </div>

      {tab === 'perfil' && <PerfilTab />}
      {tab === 'negocio' && (isDueño || isGerente) && <NegocioTab isDueño={isDueño} />}
      {tab === 'usuarios' && isDueño && <UsuariosTab />}
    </div>
  );
}

function TabButton({ active, onClick, children }: { active: boolean; onClick: () => void; children: React.ReactNode }) {
  return (
    <button
      onClick={onClick}
      className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors ${active ? 'bg-[#6A4B9F] text-white' : 'bg-white border hover:bg-slate-50'}`}
    >
      {children}
    </button>
  );
}

// ---- Perfil ----
function PerfilTab() {
  const user = authStore((s) => s.user);
  const [actual, setActual] = useState('');
  const [nueva, setNueva] = useState('');
  const [confirmar, setConfirmar] = useState('');
  const { mutate: cambiar, isPending } = useCambiarPassword();
  const { mutate: updatePerfil, isPending: savingPerfil } = useUpdatePerfil();
  const [perfil, setPerfil] = useState({ nombre: user?.nombre ?? '', apellido: user?.apellido ?? '', email: user?.email ?? '' });

  useEffect(() => {
    if (user) setPerfil({ nombre: user.nombre ?? '', apellido: user.apellido ?? '', email: user.email ?? '' });
  }, [user?.id, user?.nombre, user?.apellido, user?.email]);

  const handleCambiar = () => {
    if (!actual || !nueva || !confirmar) {
      toast.error('Completá todos los campos');
      return;
    }
    if (nueva !== confirmar) {
      toast.error('Las contraseñas no coinciden');
      return;
    }
    cambiar({ passwordActual: actual, passwordNuevo: nueva }, {
      onSuccess: () => { setActual(''); setNueva(''); setConfirmar(''); },
    });
  };

  const handleSavePerfil = () => {
    if (!perfil.nombre.trim() || !perfil.apellido.trim() || !perfil.email.trim()) {
      toast.error('Nombre, apellido y email son obligatorios');
      return;
    }
    updatePerfil({ nombre: perfil.nombre.trim(), apellido: perfil.apellido.trim(), email: perfil.email.trim() });
  };

  return (
    <div className="grid gap-6 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>Mi Perfil</CardTitle>
          <CardDescription>Actualizá tus datos personales</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex gap-3 pb-2">
            <div className="w-12 h-12 rounded-full bg-[#6A4B9F] text-white flex items-center justify-center font-bold text-lg shrink-0">
              {(perfil.nombre?.[0] || user?.email[0].toUpperCase())}{perfil.apellido?.[0] || ''}
            </div>
            <div className="text-sm">
              <p className="font-semibold">{user?.nombre} {user?.apellido}</p>
              <p className="text-muted-foreground">{user?.email}</p>
              <p className="text-xs mt-1 inline-flex px-2 py-0.5 rounded-full bg-slate-100 border">{user?.rol}</p>
            </div>
          </div>
          {user?.negocio_nombre && (
            <p className="text-sm text-muted-foreground">Negocio: <span className="font-medium text-foreground">{user.negocio_nombre}</span></p>
          )}
          <div className="space-y-2">
            <Label>Nombre</Label>
            <Input value={perfil.nombre} onChange={(e) => setPerfil({ ...perfil, nombre: e.target.value })} placeholder="Tu nombre" />
          </div>
          <div className="space-y-2">
            <Label>Apellido</Label>
            <Input value={perfil.apellido} onChange={(e) => setPerfil({ ...perfil, apellido: e.target.value })} placeholder="Tu apellido" />
          </div>
          <div className="space-y-2">
            <Label>Email</Label>
            <Input value={perfil.email} onChange={(e) => setPerfil({ ...perfil, email: e.target.value })} placeholder="tu@email.com" />
          </div>
          <Button onClick={handleSavePerfil} disabled={savingPerfil} className="w-full bg-[#6A4B9F] hover:bg-[#5A3F8A]">
            {savingPerfil ? 'Guardando...' : 'Guardar perfil'}
          </Button>
          <p className="text-xs text-muted-foreground text-center">Foto de perfil próximamente</p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Cambiar contraseña</CardTitle>
          <CardDescription>Elegí una nueva contraseña para tu cuenta</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label>Contraseña actual</Label>
            <Input type="password" value={actual} onChange={(e) => setActual(e.target.value)} placeholder="••••••••" />
          </div>
          <div className="space-y-2">
            <Label>Nueva contraseña</Label>
            <Input type="password" value={nueva} onChange={(e) => setNueva(e.target.value)} placeholder="••••••••" />
          </div>
          <div className="space-y-2">
            <Label>Confirmar nueva</Label>
            <Input type="password" value={confirmar} onChange={(e) => setConfirmar(e.target.value)} placeholder="••••••••" />
          </div>
          <Button onClick={handleCambiar} disabled={isPending} className="w-full bg-[#6A4B9F] hover:bg-[#5A3F8A]">
            {isPending ? 'Guardando...' : 'Actualizar contraseña'}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}

// ---- Negocio ----
function NegocioTab({ isDueño }: { isDueño: boolean }) {
  const { data: negocio, isLoading, error } = useNegocio();
  const { mutate: update, isPending } = useUpdateNegocio();

  const [form, setForm] = useState({
    nombre: '',
    cuit: '',
    direccion: '',
    telefono: '',
    email: '',
    puntoDeVenta: '',
    condicionVentas: '',
    logoURL: '',
    tipo: 1,
  });

  useEffect(() => {
    if (negocio) {
      setForm({
        nombre: negocio.nombre,
        cuit: negocio.cuit,
        direccion: negocio.direccion,
        telefono: negocio.telefono ?? '',
        email: negocio.email ?? '',
        puntoDeVenta: negocio.puntoDeVenta ?? '',
        condicionVentas: negocio.condicionVentas ?? '',
        logoURL: negocio.logoURL ?? '',
        tipo: tipoToInt[negocio.tipo] ?? 1,
      });
    }
  }, [negocio]);

  if (isLoading) return <Card><CardContent className="p-6">Cargando datos del negocio...</CardContent></Card>;
  if (error) return <Card><CardContent className="p-6 text-destructive">No se pudo cargar el negocio. Verificá que el backend esté corriendo.</CardContent></Card>;
  if (!negocio) return null;

  const handleSave = () => {
    if (!form.nombre.trim() || !form.cuit.trim() || !form.direccion.trim()) {
      toast.error('Nombre, CUIT y Dirección son obligatorios');
      return;
    }
    update({
      nombre: form.nombre.trim(),
      cuit: form.cuit.trim(),
      direccion: form.direccion.trim(),
      telefono: form.telefono.trim() || null as any,
      email: form.email.trim() || null as any,
      puntoDeVenta: form.puntoDeVenta.trim() || null as any,
      condicionVentas: form.condicionVentas.trim() || null as any,
      logoURL: form.logoURL.trim() || null as any,
      tipo: form.tipo,
    });
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Mi Negocio</CardTitle>
        <CardDescription>
          {isDueño ? 'Actualizá la información de tu negocio' : 'Información del negocio'}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid gap-4 md:grid-cols-2">
          <div className="space-y-2">
            <Label>Nombre / Razón social *</Label>
            <Input value={form.nombre} onChange={(e) => setForm({ ...form, nombre: e.target.value })} disabled={!isDueño} />
          </div>
          <div className="space-y-2">
            <Label>CUIT *</Label>
            <Input value={form.cuit} onChange={(e) => setForm({ ...form, cuit: e.target.value })} disabled={!isDueño} />
          </div>
          <div className="space-y-2 md:col-span-2">
            <Label>Dirección *</Label>
            <Input value={form.direccion} onChange={(e) => setForm({ ...form, direccion: e.target.value })} disabled={!isDueño} />
          </div>
          <div className="space-y-2">
            <Label>Teléfono</Label>
            <Input value={form.telefono} onChange={(e) => setForm({ ...form, telefono: e.target.value })} disabled={!isDueño} />
          </div>
          <div className="space-y-2">
            <Label>Email</Label>
            <Input value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} disabled={!isDueño} />
          </div>
          <div className="space-y-2">
            <Label>Punto de venta</Label>
            <Input value={form.puntoDeVenta} onChange={(e) => setForm({ ...form, puntoDeVenta: e.target.value })} disabled={!isDueño} />
          </div>
          <div className="space-y-2">
            <Label>Condición de venta</Label>
            <Input value={form.condicionVentas} onChange={(e) => setForm({ ...form, condicionVentas: e.target.value })} disabled={!isDueño} placeholder="Ej: Contado" />
          </div>
          <div className="space-y-2">
            <Label>Tipo de negocio</Label>
            <Select value={String(form.tipo)} onValueChange={(v) => setForm({ ...form, tipo: Number(v) })} disabled={!isDueño}>
              <SelectTrigger><SelectValue /></SelectTrigger>
              <SelectContent>
                <SelectItem value="0">Cerrajería</SelectItem>
                <SelectItem value="1">Ferretería</SelectItem>
                <SelectItem value="2">Ambos</SelectItem>
              </SelectContent>
            </Select>
            {!isDueño && <p className="text-xs text-muted-foreground">Actual: {intToTipoLabel[form.tipo] ?? form.tipo}</p>}
          </div>
          <div className="space-y-2 md:col-span-2">
            <Label>Logo URL</Label>
            <Input value={form.logoURL} onChange={(e) => setForm({ ...form, logoURL: e.target.value })} disabled={!isDueño} placeholder="https://..." />
          </div>
        </div>

        <div className="flex gap-4 text-xs text-muted-foreground border-t pt-4">
          <span>Estado: <b>{negocio.estado}</b></span>
          <span>Usuarios: <b>{negocio.totalUsuarios}</b></span>
          <span>Productos: <b>{negocio.totalProductos}</b></span>
        </div>

        {isDueño && (
          <Button onClick={handleSave} disabled={isPending} className="bg-[#6A4B9F] hover:bg-[#5A3F8A]">
            {isPending ? 'Guardando...' : 'Guardar cambios'}
          </Button>
        )}
      </CardContent>
    </Card>
  );
}

const rolToInt: Record<string, number> = { Dueño: 1, Gerente: 2, Empleado: 3 };

// ---- Usuarios (Dueño only) ----
function UsuariosTab() {
  const { data: usuarios, isLoading } = useUsuarios(true);
  const { mutate: create, isPending: creating } = useCreateUsuario();
  const { mutate: update } = useUpdateUsuario();
  const { mutate: remove } = useDeleteUsuario();
  const [open, setOpen] = useState(false);
  const [editing, setEditing] = useState<any | null>(null);
  const [form, setForm] = useState({ nombre: '', apellido: '', email: '', password: '', rol: 'Empleado' });

  const openCreate = () => {
    setEditing(null);
    setForm({ nombre: '', apellido: '', email: '', password: '', rol: 'Empleado' });
    setOpen(true);
  };
  const openEdit = (u: any) => {
    setEditing(u);
    setForm({ nombre: u.nombre, apellido: u.apellido, email: u.email, password: '', rol: u.rol });
    setOpen(true);
  };
  const handleSubmit = () => {
    if (!form.nombre.trim() || !form.apellido.trim() || !form.email.trim()) {
      toast.error('Nombre, apellido y email son obligatorios');
      return;
    }
    if (!editing && !form.password) {
      toast.error('La contraseña es obligatoria al crear');
      return;
    }
    const rolInt = rolToInt[form.rol] ?? 3;
    if (editing) {
      update({ id: editing.id, data: { nombre: form.nombre.trim(), apellido: form.apellido.trim(), rol: rolInt, activo: true } });
      setOpen(false);
    } else {
      create({ nombre: form.nombre.trim(), apellido: form.apellido.trim(), email: form.email.trim(), password: form.password, rol: rolInt }, {
        onSuccess: () => setOpen(false),
      });
    }
  };

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0">
        <div>
          <CardTitle>Usuarios del negocio</CardTitle>
          <CardDescription>Solo el Dueño puede crear, editar y desactivar usuarios</CardDescription>
        </div>
        <Button onClick={openCreate} className="bg-[#6A4B9F] hover:bg-[#5A3F8A]">Nuevo usuario</Button>
      </CardHeader>
      <CardContent className="space-y-4">
        {isLoading ? (
          <p className="text-sm text-muted-foreground">Cargando usuarios...</p>
        ) : !usuarios || usuarios.length === 0 ? (
          <p className="text-sm text-muted-foreground">No hay usuarios.</p>
        ) : (
          <div className="border rounded-lg overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-slate-50 border-b">
                  <tr>
                    <th className="text-left p-3 font-medium">Nombre</th>
                    <th className="text-left p-3 font-medium">Email</th>
                    <th className="text-left p-3 font-medium">Rol</th>
                    <th className="text-right p-3 font-medium">Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  {usuarios.map((u: any) => (
                    <tr key={u.id} className="border-b last:border-0 hover:bg-slate-50">
                      <td className="p-3">{u.nombre} {u.apellido}</td>
                      <td className="p-3 text-muted-foreground">{u.email}</td>
                      <td className="p-3"><span className="px-2 py-1 rounded-full bg-slate-100 border text-xs">{u.rol}</span></td>
                      <td className="p-3 text-right flex justify-end gap-2">
                        <Button variant="outline" size="sm" onClick={() => openEdit(u)}>Editar</Button>
                        <Button variant="outline" size="sm" onClick={() => { if (confirm(`¿Desactivar a ${u.email}?`)) remove(u.id); }} className="text-destructive hover:text-destructive">Desactivar</Button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        <Dialog open={open} onOpenChange={setOpen}>
          <DialogContent className="sm:max-w-[480px]">
            <DialogHeader>
              <DialogTitle>{editing ? 'Editar usuario' : 'Nuevo usuario'}</DialogTitle>
            </DialogHeader>
            <div className="grid gap-4 py-2">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label>Nombre *</Label>
                  <Input value={form.nombre} onChange={(e) => setForm({ ...form, nombre: e.target.value })} />
                </div>
                <div className="space-y-2">
                  <Label>Apellido *</Label>
                  <Input value={form.apellido} onChange={(e) => setForm({ ...form, apellido: e.target.value })} />
                </div>
              </div>
              <div className="space-y-2">
                <Label>Email *</Label>
                <Input value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
              </div>
              {!editing && (
                <div className="space-y-2">
                  <Label>Contraseña *</Label>
                  <Input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
                </div>
              )}
              <div className="space-y-2">
                <Label>Rol</Label>
                <Select value={form.rol ?? 'Empleado'} onValueChange={(v) => { if (v) setForm({ ...form, rol: v }); }}>
                  <SelectTrigger><SelectValue /></SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Dueño">Dueño</SelectItem>
                    <SelectItem value="Gerente">Gerente</SelectItem>
                    <SelectItem value="Empleado">Empleado</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
            <DialogFooter>
              <Button variant="outline" onClick={() => setOpen(false)}>Cancelar</Button>
              <Button onClick={handleSubmit} disabled={creating} className="bg-[#6A4B9F] hover:bg-[#5A3F8A]">{editing ? 'Guardar' : 'Crear'}</Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </CardContent>
    </Card>
  );
}
