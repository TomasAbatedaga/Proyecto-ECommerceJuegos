# ECommerce Juegos

Esta es una pagina para compra de juegos y softwares

## 👥 Integrantes del Grupo
* Tomas Abatedaga - abatedagatomas@gmail.com - @TomasAbatedaga - Discord: ztomy1
* Facundo Calderon - facu.eze.calderon@hotmail.com - @FacundoC2013 - Discord: facundoc2013

## 📐 Modelado de Datos

A continuación se presenta el esquema del modelo de datos correspondiente a la aplicación.

Diagrama Entidad-Relación (DER) / Diagrama de Clases

![](assets/diagrama.png)

## 🗄️ Base de Datos

El proyecto utiliza MySQL 8.0.16 o superior. El esquema inicial y los datos de prueba se encuentran en [database/script.sql](database/script.sql), destinado a una base nueva.

## Instrucciones para levantar la base de datos

Para inicializar la base de datos localmente de forma manual, sigue estos pasos:

1. Abre tu gestor de base de datos (DBeaver o MySQL Workbench).
2. Asegúrate de tener una conexión activa a tu servidor local de MySQL.
3. Abre un nuevo editor de scripts SQL.
4. Abre el archivo `script.sql` (ubicado en la raíz de este repositorio).
5. Copia todo el código que contiene el archivo.
6. Pega ese código en la ventana en blanco del script SQL de tu gestor de base de datos.
7. Ejecuta el script completo.
8. Actualiza la vista de tus bases de datos (presionando `F5`). Verás creada la base de datos junto con las tablas correspondientes y los datos de prueba iniciales listos para usar.
9. Abre el archivo `appsettings.json` en el proyecto de Visual Studio / VS Code y actualiza la cadena de conexión (DefaultConnection) poniendo tu usuario, contraseña y puerto de MySQL.

## 🔐 Usuarios de Prueba (Login)

El script SQL incluye dos usuarios preconfigurados para probar los distintos roles del sistema. Una vez levantada la aplicación, puedes ingresar con las siguientes credenciales:

* **Administrador** (Acceso total al sistema y gestión de usuarios)
  * **Email:** `admin@example.test`
  * **Contraseña:** `AdminPrueba123!`

* **Empleado** (Acceso restringido, gestión de entidades y perfil propio)
  * **Email:** `cliente@example.test`
  * **Contraseña:** `ClientePrueba123!`