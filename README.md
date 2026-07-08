# StarsBrawl

Proyecto base proporcionado al alumnado de la asignatura de Tecnología de Servidores y Bases de Datos en el grado de Diseño de Videojuegos en la Universidad Europea



\# StarsBrawl - Sistema de Gestión de Brawlers



Proyecto de integración de bases de datos para Unity, enfocado en la persistencia de datos de usuario y carga dinámica de activos (UI).



\## Arquitectura del Sistema

El proyecto utiliza un patrón de \*\*carga asíncrona\*\* para evitar bloqueos en el hilo principal de Unity, permitiendo una experiencia fluida incluso con consultas SQL pesadas.



\* \*\*Conector:\*\* `MySqlConnector` para comunicación directa.

\* \*\*UI:\*\* Sistema de gestión de estados (`LoadingSpinner` / `EmptyMessage`).

\* \*\*Datos:\*\* Arquitectura basada en POCOs (`DatosBrawler`) para desacoplar la base de datos de la lógica visual.



\## Instrucciones de Instalación

1\. \*\*Base de Datos:\*\* Crea la base de datos ejecutando el script `database\_setup.sql` en tu gestor MySQL (ej. HeidiSQL).

2\. \*\*Unity:\*\* Abre el proyecto en Unity 6.

3\. \*\*Configuración:\*\* Asegúrate de que el servidor MySQL local esté corriendo en el puerto 3306.

4\. \*\*Ejecución:\*\* Ejecuta la escena `Login` y accede con tu identificador para poblar la vista de Brawlers.



\## Requisitos

\* Unity 6.0 o superior.

\* MySQL Server 8.0+.

\* `MySqlConnector` instalado en el proyecto.

