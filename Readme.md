![](img/homerobot.jpg)

Se model� el ambiente como una clase Environment que contiene:

�	Arreglo de dos dimensiones de casillas (cells) que representa todo el terreno donde se sit�an y mueven los elementos. 

�	Propiedad indizada para obtener una casilla a partir de sus coordenadas.

�	Arreglo para almacenar las coordenadas de las casillas que conforman el corral (Playpen).

Lista de las casillas sucias (DirtyCells).
�	Lista de los ni�os (Children).
�	M�todos para reiniciar los elementos del ambiente.

Las casillas tienen:
�	Propiedad para almacenar su estado, para lo cual se utiliza una m�scara, pues en una casilla pueden coexistir diferentes elementos.
�	Referencias a los objetos que contienen.
�	Posici�n dentro del ambiente.
�	Posici�n dentro del corral, si formara parte de �ste.

Se defini� una clase MobileObject de la que heredan el HomeRobot, Child y Obstacle y que tiene:

�	Posici�n actual (CurrentPos) y anterior(PreviousPos) del objeto.
�	M�todos para realizar el movimiento de un objeto a una casilla determinada y la generaci�n aleatoria de la direcci�n del movimiento.
HomeRobot es una clase abstracta a partir de la cual se crea una jerarqu�a de agentes robots, en la que se incluyen los cuatro modelos de robots creados. Contiene:
�	Campo referencia al ni�o que tiene cargado
�	M�todo para soltar al ni�o con el consiguiente cambio de estado
�	M�todo para limpiar la casilla en la que se encuentra
�	M�todo abstracto Play que es que ejecuta el turno del Robot. Este m�todo se redefine en cada uno de los modelos.

RandomRobot redefine el m�todo Play para el modelo aleatorio.

CleanerRobot redefine el m�todo Play para el modelo que prioriza la limpieza.

CatcherRobot redefine el m�todo Play para el modelo que prioriza la captura de los ni�os. Implementa m�todos para preparar el camino para visitar todas las casillas sucias, detectar el ni�o m�s cercano al robot, el camino m�s corto para poner al ni�o en el corral, o sea los planes para lograr las intenciones que se traza el robot a partir del conocimiento del ambiente. 

SmartRobot redefine el m�todo Play introduciendo las mejoras propuestas al modelo del CatcherRobot.

La clase Simulation se encarga de orquestar el funcionamiento, o sea de ejecutar los turnos en los el Robot lleva a cabo sus acciones y el ambiente se modifica aleatoriamente a trav�s del movimiento de los ni�os, controlando las unidades de cambio y chequeando si se alcanzan los estados finales para detener la simulaci�n.

![](img/jerarquia-de-clases.jpg)

Todas estas clases se agruparon en la biblioteca HomeRobot.Core y se implementaron dos programas de prueba que las utilizan:

1.	Aplicaci�n de consola a la que se le pasan como par�metros un: archivo Json con las configuraciones del ambiente que se desean probar, y una cadena que especifica el tipo de agente que se va a utilizar, el cual genera para un archivo csv con los resultados de la simulaci�n, que se desean analizar.

2.	Aplicaci�n WinForms que dibuja el ambiente en la pantalla, y en cada paso se�ala en rojo la casilla donde se encuentra el objeto al que le toca su turno, y en amarillo la casilla hacia donde se va a producir el movimiento. En esta aplicaci�n se puede ejecutar paso a paso, se puede iniciar una nueva simulaci�n y se puede cambiar el tipo de agente, su implementaci�n tuvo como objetivo la retroalimentaci�n visual para la verificaci�n de los algoritmos. En esta aplicaci�n las casillas del ambiente tienen un tama�o fijo que permite situar las im�genes correspondientes a los elementos del ambiente. Cuando el ambiente no cabe completamente en la pantalla, aparecen unas barras de scroll para desplazar la imagen. 


