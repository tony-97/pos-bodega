**Sistema de Gestión de Inventario y Ventas (Punto de Venta)**  

> * **Descripción del Negocio:** Bodega de barrio dedicada al abastecimiento diario de familias de la zona. Maneja un alto volumen de transacciones rápidas y cuenta con clientes habituales a los que, en ocasiones, se les otorga crédito directo (fiado).

# **2: Planteamiento de la Problemática**

**Problemas actuales en la gestión de información:**

> 1. **Control Manual y Riesgo de Pérdida:** Todas las ventas y compras se registran en un cuaderno. Es un proceso lento, propenso a errores humanos de cálculo y con alto riesgo de pérdida o deterioro de la información.  
> 2. **Descuadre y Desconocimiento del Stock:** No se sabe con exactitud cuánta mercadería hay en tiempo real. Esto genera "quiebres de stock" (el cliente pide algo que ya se acabó sin que el dueño lo notara) o sobre-compras de productos que aún no rotan.  
> 3. **Gestión de Fiados Desordenada:** Las cuentas por cobrar (fiados) se mezclan con otras anotaciones, dificultando saber exactamente cuánto debe un vecino y desde hace cuánto tiempo, lo que afecta la liquidez del negocio.  
> 4. **Demora en la Atención (Horas Pico):** Calcular mentalmente o con calculadora los totales y los vueltos retrasa la atención cuando hay varios clientes esperando.

# **3: Definición de Procesos de Negocio (1 y 2\)**

**1\. Proceso de Abastecimiento / Compras**

> * **Descripción:** Registro del ingreso de nueva mercadería al local. Este es el único proceso que incrementa el stock en el sistema.  
> * **Entradas:** Datos del Proveedor, Lista de productos comprados (Categoría, Nombre, Cantidad comprada, Costo unitario).  
> * **Salidas:** Nota de Entrada en sistema, actualización (suma) automática del stock de los productos.

**2\. Proceso de Ventas (Punto de Venta \- POS)**

> * **Descripción:** Registro rápido de la salida de productos hacia los clientes, cálculo del total y registro del método de pago.  
> * **Entradas:** Escaneo o búsqueda del producto, Cantidad a vender, Método de pago (Efectivo/Yape/Plin), Datos del cliente (solo si el monto es \> S/ 700 o si es venta al fiado).  
> * **Salidas:** Comprobante de pago (Boleta electrónica o Ticket interno), actualización (resta) automática del stock.

# **4: Definición de Procesos de Negocio (3 y 4\)**

**3\. Proceso de Control de "Fiados" (Cuentas por Cobrar)**

> * **Descripción:** Administración de los créditos directos otorgados a clientes de confianza y el registro de sus amortizaciones/pagos.  
> * **Entradas:** Selección de Cliente registrado, Monto de la nueva deuda (derivado de una venta) o Monto del pago a cuenta realizado.  
> * **Salidas:** Saldo actualizado del cliente, Historial de pagos, Alertas de deudas vencidas.

**4\. Proceso de Alertas y Reportes de Inventario**

> * **Descripción:** Monitoreo del estado del negocio y advertencia de productos que están a punto de agotarse.  
> * **Entradas:** Límite de stock mínimo predefinido para cada producto, Rango de fechas para reportes.  
> * **Salidas:** Reporte visual de "Productos con Stock Bajo", Reporte de ventas del día/caja.

# **5: Requerimientos de Software \- Funcionales (Parte 1\)**


| ID | Nombre | Descripción | Prioridad | Actor | Precondiciones | Reglas de negocio | Criterios de aceptación |
| :---- | :---- | :---- | :---- | :---- | :---- | :---- | :---- |
| **RF1** | Registrar Venta | El sistema debe permitir buscar productos, agregarlos al carrito, calcular el total y procesar el pago. | Alta | Vendedor | Tener caja abierta y sesión iniciada. | El stock del producto no puede quedar en negativo. Si es NRUS, solo emite Boleta. | Se guarda la venta, se descuenta el stock en la BD y se genera el ticket en menos de 3 seg. |
| **RF2** | Registrar Compra | El sistema debe permitir registrar el ingreso de mercadería indicando proveedor, costo y cantidad. | Alta | Admin | El proveedor debe existir en el sistema. | Solo el Administrador puede ingresar compras. | El stock del producto aumenta exactamente en la cantidad ingresada. |

# **6: Requerimientos de Software \- Funcionales (Parte 2\)**

| ID | Nombre | Descripción | Prioridad | Actor | Precondiciones | Reglas de negocio | Criterios de aceptación |
| :---- | :---- | :---- | :---- | :---- | :---- | :---- | :---- |
| **RF3** | Alerta Stock Mínimo | El dashboard principal debe listar los productos cuyo stock actual sea igual o menor al límite establecido. | Media | Admin | Productos deben tener configurado su campo "stock\_minimo". | La alerta es visual (colores) y automática. | Mostrar tabla con producto, stock actual y stock mínimo, ordenado de menor a mayor. |
| **RF4** | Pago de Fiado | El sistema debe permitir buscar a un cliente con deuda y registrar un abono a su saldo. | Alta | Admin / Vendedor | El cliente debe tener saldo deudor \> 0\. | El pago no puede ser mayor a la deuda total. | El saldo del cliente disminuye según el monto pagado; se genera un recibo de pago. |

# **7: Requerimientos de Software \- No Funcionales**

| ID | Nombre | Descripción |
| :---- | :---- | :---- |
| **RFN1** | Arquitectura Local (Disponibilidad) | Debido a las intermitencias de internet en la zona, el sistema debe ser una aplicación de escritorio conectada a una base de datos local SQLite para no detener las ventas. |
| **RFN2** | Seguridad y Roles | El sistema debe tener un Login que encripte las contraseñas. Debe restringir los módulos: el Vendedor no puede acceder al registro de Compras ni a los Reportes de Ganancias. |
| **RFN3** | Usabilidad (Velocidad de operación) | La interfaz del Punto de Venta (POS) debe permitir registrar una venta usando atajos de teclado o escáner, requiriendo menos de 4 clics en total para concretar una venta rápida. |