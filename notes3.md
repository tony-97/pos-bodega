Revisé tu guía metodológica y la apliqué a tu caso específico. Antes de armar el plan, verifiqué el estado actual de algunas cosas que cambian rápido en Perú (régimen tributario, facturación electrónica, medios de pago digitales) para que las recomendaciones sean concretas y no genéricas. Asumo que es una bodega de barrio típica —formal o en proceso de formalizarse— salvo que me digas lo contrario.

## 1. Diagnóstico inicial (según el marco de tu guía)

Con casi total seguridad estás en el escenario **1 o 2** de tu sección 2.1: "Inexistencia de Sistema Informático" (cuaderno) o "Sistema Obsoleto" (Excel suelto, celular con notas). Antes de diseñar nada, tu propia guía exige levantar los datos reales del negocio. Adaptando su checklist a una bodega:

- Nombre/razón social y **RUC + régimen tributario exacto** (esto no es un detalle menor, ver punto 2a)
- Rubro específico: ¿abarrotes puros, mixta con licores, con recarga de celulares?
- ¿Vende al fiado? ¿A cuántos clientes fijos?
- Medios de pago que ya acepta (efectivo, Yape, Plin, tarjeta)
- Cuántas personas atienden (¿solo el dueño, o hay cajeros?)
- Qué usa hoy para "llevar cuentas" (cuaderno, Excel, nada)

## 2. Factores de contexto que van a moldear el diseño

### a) Régimen tributario y comprobantes — el punto que más cambia la arquitectura

Por el tamaño típico de una bodega, lo más probable es que esté (o le convenga estar) en el **Nuevo RUS**:

| Categoría | Ventas/compras mensuales (máx.) | Cuota mensual | Comprobante permitido |
|---|---|---|---|
| 1 | S/ 5,000 | S/ 20 | Boleta de venta |
| 2 | S/ 8,000 | S/ 50 | Boleta de venta |

Si en cualquier mes las ventas o compras superan los S/ 8,000, el negocio debe pasar a un régimen distinto al NRUS (Especial, MYPE o General), que sí permite emitir facturas con IGV desagregado. Los contribuyentes del NRUS solo pueden emitir boletas de venta, nunca facturas.

Esto tiene una implicación de **diseño**, no solo legal: hoy en día casi toda la emisión de comprobantes en Perú es electrónica. Incluso en NRUS, la recomendación práctica es asumir que todo va electrónico y configurar el sistema así desde el día uno. Dos datos operativos que tu módulo de ventas debe respetar: a partir de S/ 700 la boleta electrónica debe identificar al comprador con DNI, y por debajo de S/ 5 no es obligatorio emitir comprobante salvo que el cliente lo pida.

**Decisión de arquitectura clave:** no construyas tu propio motor de facturación electrónica desde cero (requiere firma digital, XML UBL, validación en tiempo real con SUNAT — complejidad innecesaria para este proyecto). Las opciones reales son (1) que el sistema registre la venta internamente y el usuario emita la boleta aparte en el portal gratuito SEE-SOL o la app Emprender de SUNAT, o (2) integrar tu sistema con un PSE/OSE de terceros vía API para que la emisión sea automática. Para un proyecto académico o un primer MVP, la opción 1 es más realista; para producción real, la opción 2 evita doble digitación (que es justo lo que tu guía advierte que hace perder contra el cuaderno).

*Nota: confirma el régimen y estos montos con el dueño/contador antes de cerrar el diseño — son datos que cambian y aquí doy el panorama general, no asesoría tributaria vinculante.*

### b) Medios de pago

Perú viene implementando interoperabilidad de QR: un solo código puede aceptar pagos de Yape, Plin y otras billeteras, así que no necesitas diseñar métodos de pago separados por app. Sin embargo, la mayoría de sistemas POS pequeños no tienen integración directa con Yape/Plin: el cajero verifica el pago en su celular y lo registra manualmente. Diseña el campo "método de pago" como un simple selector (Efectivo / Yape-Plin-QR / Tarjeta) con conciliación manual — intentar una integración automática con las billeteras es sobre-ingeniería para este proyecto.

### c) Conectividad y energía en Huánuco

Huánuco se ha ubicado cerca del promedio nacional de velocidad de internet móvil (alrededor de 12 Mbps), pero en algunos periodos ha mostrado una tasa de pérdida de paquetes de datos más alta que regiones como Lima o Callao, lo que se traduce en cortes o intermitencia, especialmente fuera del centro urbano. No es un escenario de "no hay internet", pero tampoco es Lima: la conexión es menos predecible. Esto refuerza exactamente la crítica que tu propia guía hace a la "moda web" (sección 5): un sistema 100% dependiente de conexión constante es un riesgo real para este tipo de negocio. Súmale que los cortes de luz siguen siendo una posibilidad en negocios pequeños de provincia — el sistema debe guardar cada transacción al instante, no al cerrar caja.

### d) El cuaderno y el fiado — tu verdadera competencia

Como bien dice tu guía, el rival no es otro software, es el cuaderno. Y en una bodega peruana, el cuaderno no solo registra ventas: registra **fiados** (crédito informal a vecinos de confianza). Si tu sistema no cubre esto, el dueño seguirá usando el cuaderno en paralelo para esa función — y el proyecto fracasa en su propio objetivo. Vale la pena tratarlo como módulo de primera clase, no como extra.

### e) Soporte técnico regional

Fuera de Lima hay menos proveedores especializados en soporte de sistemas de punto de venta. Esto pesa a favor de una solución simple de mantener (evitar dependencias raras, documentar bien, preferir tecnología estándar) sobre una arquitectura sofisticada que nadie más pueda dar soporte si tú no estás disponible.

## 3. Plan de diseño del sistema

### Alcance funcional (módulos)

- **Productos e inventario**: alta/baja, categorías, código de barras *opcional* (muchos productos de bodega no lo traen o son productos sueltos/a granel), unidad de medida (unidad, kg, litro, fracción), stock actual, stock mínimo con alerta de reposición
- **Compras (Notas de Entrada)**: registrar ingreso de mercadería por proveedor → incrementa stock automáticamente
- **Ventas / POS (Comprobantes de Salida)**: buscar producto (por código o nombre, con "venta rápida" para ítems no catalogados), calcular total, método de pago, generar boleta → decrementa stock automáticamente
- **Fiados / cuentas por cobrar**: cliente, límite de crédito, historial de pagos, saldo pendiente, alertas de deuda antigua
- **Usuarios y perfiles**: Administrador (dueño, acceso total) vs. Cajero (solo operación diaria)
- **Reportes**: ventas del día/semana/mes, productos más vendidos, ganancia bruta, stock bajo, cuentas por cobrar

**Regla dura, tomada directo de tu guía**: el stock nunca se edita manualmente. Solo cambia vía documento de Compra o de Venta. Esto es lo que garantiza trazabilidad para una eventual auditoría.

### Requisitos no funcionales

- Contraseñas alfanuméricas, accesos diferenciados por perfil
- Guardado inmediato de cada transacción (no al cierre de caja) — mitiga cortes de luz
- Copia de seguridad automática, local como mínimo, con opción de sincronizar a la nube cuando haya conexión
- Registro de qué usuario hizo qué acción (bitácora básica)
- El criterio de usabilidad más importante: **una venta completa debe tomar menos tiempo que anotarla en el cuaderno**. Si no, el sistema será rechazado en la práctica.

### Modelo de datos (entidades clave)

Sin diagrama, como pediste — solo la lista de entidades y qué guardan:

- **Producto**: id, nombre, categoría, unidad de medida, código de barras (opcional), costo, precio de venta, stock actual, stock mínimo
- **Categoría**: id, nombre
- **Usuario**: id, nombre, rol (Administrador/Cajero), credenciales
- **Cliente**: id, nombre — *solo se crea si tiene fiado*; no captures datos que no vas a usar (mismo principio que tu guía aplica al campo "sexo")
- **Proveedor**: id, nombre, contacto
- **Compra**: cabecera (fecha, proveedor, usuario) + detalle (producto, cantidad, costo unitario)
- **Venta**: cabecera (fecha, cliente opcional, usuario, método de pago, tipo de comprobante, total) + detalle (producto, cantidad, precio unitario)
- **Pago_Fiado**: cliente, fecha, monto, saldo restante

### Selección de plataforma

Dado el punto 2c (conectividad variable, cortes de luz posibles) y 2e (soporte técnico limitado), la recomendación —aplicando el criterio de tu propia sección 5— es **escritorio con base de datos local como núcleo**, más un módulo ligero de sincronización a la nube (para backup y para el envío del comprobante electrónico) que se ejecute cuando haya conexión, no en tiempo real obligatorio. Web puro solo tendría sentido si confirmas que el local tiene internet estable y planea abrir más de una sucursal pronto — si es una sola bodega, no se justifica el costo recurrente de hosting.

### Hoja de ruta por fases

| Fase | Objetivo | Entregable |
|---|---|---|
| 0. Levantamiento | Visita al local, entrevista al dueño, confirmar régimen tributario e inventario inicial | Ficha de datos del negocio |
| 1. Diseño de datos y arquitectura | Definir entidades, plataforma, estrategia de comprobante electrónico (portal vs. PSE) | Modelo de datos + decisión de arquitectura |
| 2. Módulo de productos | CRUD de productos, categorías, unidades de medida, stock mínimo | Inventario funcionando |
| 3. Módulo de compras | Registrar notas de entrada → stock sube | Flujo compra-stock probado |
| 4. Módulo de ventas / POS | Venta, método de pago, emisión de boleta | Flujo venta-stock-comprobante probado |
| 5. Fiados y reportes | Crédito a clientes, reportes de venta/ganancia/stock bajo | Módulo de crédito + reportes básicos |
| 6. Seguridad | Roles, contraseñas, bitácora | Login con permisos diferenciados |
| 7. Pruebas con datos reales | Correr el sistema en paralelo al cuaderno 1-2 semanas | Comparativo de velocidad sistema vs. cuaderno |
| 8. Capacitación y despliegue | Entrenar al dueño/cajero, migrar datos, feedback continuo | Sistema en producción |

### Criterios de aceptación

Tomados de la sección 6 de tu guía, aplicados a este caso:

1. Exactitud en los totales (y en el cálculo de cuota NRUS vs. IGV desagregado, según el régimen real del cliente)
2. La boleta se emite y queda **realmente aceptada** por SUNAT, no solo generada localmente
3. El stock sube con cada compra y baja con cada venta, sin intervención manual
4. Los accesos respetan el perfil (Cajero no puede ver ni tocar lo que solo le corresponde al Administrador)
5. Prueba cronometrada: una venta típica en el sistema debe ser claramente más rápida que anotarla en el cuaderno

Si quieres, puedo profundizar en el diseño de tablas y campos del módulo de ventas o del módulo de fiados, o ayudarte a redactar los requerimientos funcionales en formato de historias de usuario.