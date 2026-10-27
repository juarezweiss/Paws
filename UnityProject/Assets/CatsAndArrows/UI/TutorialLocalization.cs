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
                "Row and column numbers show how many paws belong there.",
                "Os números das linhas e colunas mostram quantas patas há em cada uma.",
                "Los números de filas y columnas indican cuántas patas hay en cada una."
            } },
            { "tutorial.begin", new[] {
                "Let's find the paws together, one step at a time.",
                "Vamos encontrar as patas juntos, um passo de cada vez.",
                "Busquemos las patas juntos, paso a paso."
            } },
            { "tutorial.states-intro", new[] {
                "Empty cells are editable and have three possible states.",
                "As células vazias podem ser alteradas e têm três estados possíveis.",
                "Las celdas vacías se pueden cambiar y tienen tres estados posibles."
            } },
            { "tutorial.empty-state", new[] {
                "<b>Empty</b>: undecided.",
                "<b>Vazia</b>: sem decisão.",
                "<b>Vacía</b>: sin decidir."
            } },
            { "tutorial.x-state", new[] {
                "<b>X</b>: this cell does not contain a paw.",
                "<b>X</b>: esta célula não contém uma pata.",
                "<b>X</b>: esta celda no contiene una pata."
            } },
            { "tutorial.paw-state", new[] {
                "<b>Paw</b>: this cell contains a paw.",
                "<b>Pata</b>: esta célula contém uma pata.",
                "<b>Pata</b>: esta celda contiene una pata."
            } },
            { "tutorial.hatched", new[] {
                "Hatched cells have no paws and cannot be changed.",
                "As células hachuradas não têm patas e não podem ser alteradas.",
                "Las celdas rayadas no tienen patas y no se pueden cambiar."
            } },
            { "tutorial.arrows", new[] {
                "Arrows stay fixed. Each points toward at least one paw somewhere ahead on its straight path, including diagonals.",
                "As setas são fixas. Cada uma aponta para pelo menos uma pata em algum ponto à frente, no seu trajeto em linha reta, inclusive nas diagonais.",
                "Las flechas son fijas. Cada una apunta hacia al menos una pata en algún punto por delante, en su trayectoria recta, también en diagonal."
            } },
            { "tutorial.practice", new[] {
                "<b>Tap</b> the outlined cell 3 times.\nEmpty → X → Paw → Empty.",
                "<b>Toque</b> 3 vezes na célula contornada.\nVazia → X → Pata → Vazia.",
                "<b>Toca</b> 3 veces la celda delineada.\nVacía → X → Pata → Vacía."
            } },
            { "tutorial.zero-column", new[] {
                "This column has 0 paws.\nStart on an <b>Empty</b> cell and <b>swipe</b> down to mark X.",
                "Esta coluna tem 0 patas.\nComece em uma célula <b>Vazia</b> e <b>arraste</b> para baixo para marcar X.",
                "Esta columna tiene 0 patas.\nEmpieza en una celda <b>Vacía</b> y <b>desliza</b> hacia abajo para marcar X."
            } },
            { "tutorial.zero-row", new[] {
                "This row has 0 paws.\nStart on an <b>Empty</b> cell and <b>swipe</b> right to mark X.",
                "Esta linha tem 0 patas.\nComece em uma célula <b>Vazia</b> e <b>arraste</b> para a direita para marcar X.",
                "Esta fila tiene 0 patas.\nEmpieza en una celda <b>Vacía</b> y <b>desliza</b> hacia la derecha para marcar X."
            } },
            { "tutorial.first-paw", new[] {
                "This arrow’s only space must be a paw. <b>Tap</b> it twice.",
                "Só há uma célula à frente da seta. <b>Toque</b> 2 vezes para uma pata.",
                "Solo hay una celda delante de la flecha. <b>Toca</b> 2 veces para una pata."
            } },
            { "tutorial.first-column-x", new[] {
                "We've found this column's only paw.\nStart on an <b>Empty</b> cell and <b>swipe</b> down to mark X.",
                "Já encontramos a única pata desta coluna.\nComece em uma célula <b>Vazia</b> e <b>arraste</b> para baixo para marcar X.",
                "Ya encontramos la única pata de esta columna.\nEmpieza en una celda <b>Vacía</b> y <b>desliza</b> hacia abajo para marcar X."
            } },
            { "tutorial.second-paw", new[] {
                "This arrow also has just one space ahead. <b>Tap</b> it twice for a paw.",
                "Outra seta com só uma célula à frente. <b>Toque</b> 2 vezes para uma pata.",
                "Otra flecha con una sola celda delante. <b>Toca</b> 2 veces para una pata."
            } },
            { "tutorial.second-row-x", new[] {
                "We've found this row's only paw.\nStart on an <b>Empty</b> cell and <b>swipe</b> right to mark X.",
                "Já encontramos a única pata desta linha.\nComece em uma célula <b>Vazia</b> e <b>arraste</b> para a direita para marcar X.",
                "Ya encontramos la única pata de esta fila.\nEmpieza en una celda <b>Vacía</b> y <b>desliza</b> hacia la derecha para marcar X."
            } },
            { "tutorial.diagonal-paw", new[] {
                "<b>Press and hold</b> the outlined arrow to see its path.\nRelease, then <b>tap</b> the only available paw cell twice.",
                "<b>Pressione e segure</b> a seta contornada para ver seu trajeto.\nSolte e <b>toque</b> 2 vezes na única célula possível para uma pata.",
                "<b>Mantén pulsada</b> la flecha delineada para ver su trayectoria.\nSuelta y <b>toca</b> 2 veces la única celda posible para una pata."
            } },
            { "tutorial.fourth-paw", new[] {
                "This row needs 2 paws. We've already found one paw.\n<b>Tap</b> the only remaining cell twice.",
                "Esta linha precisa de 2 patas. Já encontramos uma pata.\n<b>Toque</b> 2 vezes na única célula restante.",
                "Esta fila necesita 2 patas. Ya encontramos una pata.\n<b>Toca</b> 2 veces la única celda restante."
            } },
            { "tutorial.single-x", new[] {
                "We've found this column's only paw.\n<b>Tap</b> the remaining Empty cell once for X.",
                "Já encontramos a única pata desta coluna.\n<b>Toque</b> uma vez na célula Vazia restante para marcar X.",
                "Ya encontramos la única pata de esta columna.\n<b>Toca</b> una vez la celda Vacía restante para marcar X."
            } },
            { "tutorial.final-paw", new[] {
                "This column needs 2 paws. We've already found one paw.\n<b>Tap</b> twice for the last paw.",
                "Esta coluna precisa de 2 patas. Já encontramos uma pata.\n<b>Toque</b> 2 vezes para marcar a última pata.",
                "Esta columna necesita 2 patas. Ya encontramos una pata.\n<b>Toca</b> 2 veces para marcar la última pata."
            } },
            { "tutorial.independent", new[] {
                "Now try it yourself! Find all the paws on this same board.",
                "Agora é sua vez! Encontre todas as patas neste mesmo tabuleiro.",
                "¡Ahora te toca! Encuentra todas las patas en este mismo tablero."
            } },
            { "tutorial.complete", new[] {
                "All paws found! Ready for a random puzzle?",
                "Você encontrou todas as patas! Vamos jogar um tabuleiro aleatório?",
                "¡Encontraste todas las patas! ¿Jugamos un tablero aleatorio?"
            } }
        };

        internal static void AddMessages(IDictionary<string, string> messages, int localeIndex)
        {
            foreach (var entry in Messages) messages.Add(entry.Key, entry.Value[localeIndex]);
        }
    }
}
