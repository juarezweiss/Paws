using System.Collections.Generic;

namespace CatsAndArrows.UI
{
    /// <summary>Issue #26: short, fixed-board lessons, in catalog locale order en / pt-BR / es.</summary>
    internal static class TutorialLocalization
    {
        private static readonly Dictionary<string, string[]> Messages = new Dictionary<string, string[]>
        {
            { "tutorial.title", new[] { "Tutorial", "Tutorial", "Tutorial" } },
            { "tutorial.continue", new[] { "Continue", "Continuar", "Continuar" } },
            { "tutorial.start", new[] { "Let's start", "Vamos começar", "Empecemos" } },
            { "tutorial.skip", new[] { "Skip", "Pular", "Omitir" } },
            { "tutorial.play", new[] { "Play", "Jogar", "Jugar" } },
            { "tutorial.continue-guidance", new[] {
                "Tap Continue.",
                "Toque em Continuar.",
                "Toca Continuar."
            } },
            { "tutorial.start-guidance", new[] {
                "Tap Let's start.",
                "Toque em Vamos começar.",
                "Toca Empecemos."
            } },
            { "tutorial.goal-title", new[] {
                "Find every paw!",
                "Encontre todas as patas!",
                "¡Encuentra todas las patas!"
            } },
            { "tutorial.welcome", new[] {
                "Welcome to Paws!",
                "Boas-vindas a Paws!",
                "¡Te damos la bienvenida a Paws!"
            } },
            { "tutorial.goal", new[] {
                "Numbers in each row and column show how many paws are there.",
                "Os números em cada linha e coluna mostram quantas patas há ali.",
                "Los números de cada fila y columna indican cuántas patas hay allí."
            } },
            { "tutorial.begin", new[] {
                "Let's find the paws together, one step at a time.",
                "Vamos encontrar as patas juntos, um passo de cada vez.",
                "Busquemos las patas juntos, paso a paso."
            } },
            { "tutorial.states-intro", new[] {
                "You can <b>tap</b> empty cells to change their state.",
                "Você pode tocar nas células vazias para mudar seu estado.",
                "Puedes tocar las celdas vacías para cambiar su estado."
            } },
            { "tutorial.empty-state", new[] {
                "<b>Empty</b>: undecided.",
                "<b>Vazia</b>: sem decisão.",
                "<b>Vacía</b>: sin decidir."
            } },
            { "tutorial.x-state", new[] {
                "<b>X</b>: I'm sure this cell doesn't have a paw.",
                "<b>X</b>: tenho certeza de que esta célula não tem uma pata.",
                "<b>X</b>: sé que esta celda no tiene una pata."
            } },
            { "tutorial.paw-state", new[] {
                "<b>Paw</b>: I'm sure this cell has a paw.",
                "<b>Pata</b>: tenho certeza de que esta célula tem uma pata.",
                "<b>Pata</b>: sé que esta celda tiene una pata."
            } },
            { "tutorial.hatched", new[] {
                "Hatched cells have no paws and cannot be changed.",
                "As células hachuradas não têm patas e não podem ser alteradas.",
                "Las celdas rayadas no tienen patas y no se pueden cambiar."
            } },
            { "tutorial.arrows", new[] {
                "Fixed arrows point toward at least one paw in their path, even diagonally.",
                "Setas fixas apontam para ao menos uma pata no seu trajeto, inclusive na diagonal.",
                "Las flechas fijas apuntan hacia al menos una pata en su trayecto, también en diagonal."
            } },
            { "tutorial.practice", new[] {
                "<b>Tap</b> the outlined cell 3 times\nto loop through its state.",
                "<b>Toque</b> 3 vezes na célula contornada\npara percorrer seus estados.",
                "<b>Toca</b> 3 veces la celda delineada\npara recorrer sus estados."
            } },
            { "tutorial.zero-column", new[] {
                "This column has 0 paws.\nStart on an <b>Empty</b> cell and <b>swipe</b> down to mark X.",
                "Esta coluna tem 0 patas.\nComece em uma célula <b>Vazia</b> e <b>arraste</b> para baixo para marcar X.",
                "Esta columna tiene 0 patas.\nEmpieza en una celda <b>Vacía</b> y <b>desliza</b> hacia abajo para marcar X."
            } },
            { "tutorial.zero-row", new[] {
                "This row has 0 paws.\nStart on an <b>Empty</b> cell and <b>swipe</b> right to mark X.",
                "Esta linha tem 0 patas.\nComece em uma célula <b>Vazia</b> e <b>arraste</b> para a direita para marcar X.",
                "Esta fila tiene 0 patas.\nDesde una celda <b>Vacía</b>, <b>desliza</b> a la derecha para X."
            } },
            { "tutorial.first-paw", new[] {
                "There's only one space here. <b>Tap</b> it twice to put a paw.",
                "Só há um espaço aqui. <b>Toque</b> 2 vezes para colocar uma pata.",
                "Solo hay un espacio aquí. <b>Toca</b> 2 veces para poner una pata."
            } },
            { "tutorial.first-column-x", new[] {
                "We've found the only paw in this column.\nStart on an <b>Empty</b> cell and <b>swipe</b> down to mark X.",
                "Encontramos a única pata desta coluna.\nDe uma célula <b>Vazia</b>, <b>arraste</b> para baixo para X.",
                "Encontramos la única pata de esta columna.\nDesde una celda <b>Vacía</b>, <b>desliza</b> abajo para X."
            } },
            { "tutorial.second-paw", new[] {
                "There's also only one space here. <b>Tap</b> it twice to put a paw.",
                "Também só há um espaço aqui. <b>Toque</b> 2 vezes para colocar uma pata.",
                "Aquí también hay un solo espacio. <b>Toca</b> 2 veces para poner una pata."
            } },
            { "tutorial.second-row-x", new[] {
                "We've found the only paw in this row.\nStart on an <b>Empty</b> cell and <b>swipe</b> right to mark X.",
                "Encontramos a única pata desta linha.\nDe uma célula <b>Vazia</b>, <b>arraste</b> à direita para X.",
                "Encontramos la única pata de esta fila.\nDesde una celda <b>Vacía</b>, <b>desliza</b> a la derecha para X."
            } },
            { "tutorial.diagonal-paw", new[] {
                "<b>Press and hold</b> the arrow to see its path.\nRelease; <b>tap</b> twice on the only paw cell.",
                "<b>Pressione e segure</b> a seta: veja o trajeto.\nSolte; <b>toque</b> 2 vezes na única célula com pata.",
                "<b>Mantén pulsada</b> la flecha para ver su línea.\nSuelta; <b>toca</b> 2 veces la única celda con pata."
            } },
            { "tutorial.fourth-paw", new[] {
                "This row needs 2 paws. We've already found one.\n<b>Tap</b> the only remaining cell twice.",
                "Esta linha precisa de 2 patas. Já encontramos uma.\n<b>Toque</b> 2 vezes na única célula restante.",
                "Esta fila necesita 2 patas. Ya encontramos una.\n<b>Toca</b> 2 veces la única celda restante."
            } },
            { "tutorial.single-x", new[] {
                "We've found the only paw in this column.\n<b>Tap</b> the remaining <b>Empty</b> cell once for X.",
                "Encontramos a única pata desta coluna.\n<b>Toque</b> uma vez na célula Vazia restante para X.",
                "Encontramos la única pata de esta columna.\n<b>Toca</b> una vez la celda Vacía restante para X."
            } },
            { "tutorial.final-paw", new[] {
                "This column needs 2 paws. We've already found one.\n<b>Tap</b> twice for the last paw.",
                "Esta coluna precisa de 2 patas. Já encontramos uma.\n<b>Toque</b> 2 vezes para a última pata.",
                "Esta columna necesita 2 patas. Ya encontramos una.\n<b>Toca</b> 2 veces para la última pata."
            } },
            { "tutorial.independent", new[] {
                "Now try it yourself! Find all the paws on this same board.",
                "Agora é sua vez! Encontre todas as patas neste mesmo tabuleiro.",
                "¡Ahora te toca! Encuentra todas las patas en este mismo tablero."
            } },
            { "tutorial.complete", new[] {
                "We found all paws! Ready for a random puzzle?",
                "Encontramos todas as patas! Vamos jogar um tabuleiro aleatório?",
                "¡Encontramos todas las patas! ¿Jugamos un tablero aleatorio?"
            } }
        };

        internal static void AddMessages(IDictionary<string, string> messages, int localeIndex)
        {
            foreach (var entry in Messages) messages.Add(entry.Key, entry.Value[localeIndex]);
        }
    }
}
