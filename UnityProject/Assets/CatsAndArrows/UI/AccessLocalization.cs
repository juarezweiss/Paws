using System.Collections.Generic;

namespace CatsAndArrows.UI
{
    // #103: information/recovery remains available without a gameplay or assent bypass.
    public static class AccessLocalization
    {
        public static void AddMessages(IDictionary<string, string> messages, int locale)
        {
            var extra = new Dictionary<string, string[]>
            {
                { "access.title", new[] { "Access to Paws", "Acesso ao Paws", "Acceso a Paws" } },
                { "access.recheck", new[] { "Check again", "Verificar novamente", "Volver a comprobar" } },
                { "access.system-settings", new[] { "Open iPhone Settings", "Abrir Ajustes do iPhone", "Abrir Ajustes del iPhone" } },
                { "access.information", new[] { "Access, privacy & help", "Acesso, privacidade e ajuda", "Acceso, privacidad y ayuda" } },
                { "access.status.Allowed", new[] { "You can play.", "Você pode jogar.", "Puedes jugar." } },
                { "access.status.Checking", new[] { "Checking access with Apple…", "Verificando o acesso com a Apple…", "Comprobando el acceso con Apple…" } },
                { "access.status.AgeRestricted", new[] {
                    "The age information shared with Paws does not meet its minimum age. If that information is incorrect, update Age Range for Apps in your Apple Account settings, then check again.",
                    "A informação de idade compartilhada com o Paws não atende à idade mínima. Se ela estiver incorreta, atualize Faixa Etária para Apps nos ajustes da sua Conta Apple e verifique novamente.",
                    "La información de edad compartida con Paws no cumple su edad mínima. Si es incorrecta, actualiza el intervalo de edad para apps en los ajustes de tu cuenta de Apple y vuelve a comprobar." } },
                { "access.status.PermissionPending", new[] {
                    "A required parent or guardian permission is still pending. You can check again after they respond.",
                    "Uma permissão necessária de um responsável ainda está pendente. Você pode verificar novamente após a resposta.",
                    "Sigue pendiente un permiso necesario de tu madre, padre o tutor. Puedes volver a comprobar cuando responda." } },
                { "access.status.PermissionDenied", new[] {
                    "A required permission was not granted. A parent or guardian can review the request. Then check again.",
                    "Uma permissão necessária não foi concedida. Um responsável pode revisar a solicitação. Depois, verifique novamente.",
                    "No se concedió un permiso necesario. Tu madre, padre o tutor puede revisar la solicitud. Después, vuelve a comprobar." } },
                { "access.status.UpdateAcknowledgementRequired", new[] {
                    "A required update notice still needs your acknowledgement. Check again to review it with Apple.",
                    "Você ainda precisa confirmar a leitura de um aviso obrigatório de atualização. Verifique novamente para revisá-lo com a Apple.",
                    "Aún debes confirmar que has leído un aviso obligatorio de actualización. Vuelve a comprobar para revisarlo con Apple." } },
                { "access.status.RecheckRequired", new[] {
                    "An access check still needs to be completed. Check again, or review Age Range for Apps in your Apple Account settings. Your game preferences have been kept.",
                    "Uma verificação de acesso ainda precisa ser concluída. Verifique novamente ou revise Faixa Etária para Apps nos ajustes da sua Conta Apple. Suas preferências de jogo foram mantidas.",
                    "Aún debe completarse una comprobación de acceso. Vuelve a comprobar o revisa el intervalo de edad para apps en los ajustes de tu cuenta de Apple. Se han conservado tus preferencias de juego." } },
                { "access.information-body", new[] {
                    "Access rules\nYou must be at least 16 in the European Union, or 13 elsewhere. If you are under 18, you also need permission from your parent or legal guardian. Their permission does not override the minimum age.\n\nPrivacy\nPaws uses Apple's native age and permission features when needed. Paws does not save birthdates, ID documents or Apple's age responses.\n\nHelp\nTo review shared age information, open Settings → your Apple Account → Personal Information → Age Range for Apps. After a correction or permission response, return to Paws and choose Check again. For publisher support and published policies, use App Support on Paws' App Store page.",
                    "Regras de acesso\nVocê precisa ter pelo menos 16 anos na União Europeia ou 13 anos nos demais países. Se tiver menos de 18 anos, também precisa da permissão de um responsável legal. Essa permissão não substitui a idade mínima.\n\nPrivacidade\nO Paws usa os recursos nativos de idade e permissão da Apple quando necessário. O Paws não salva datas de nascimento, documentos de identidade ou respostas de idade da Apple.\n\nAjuda\nPara revisar as informações de idade compartilhadas, abra Ajustes → sua Conta Apple → Informações Pessoais → Faixa Etária para Apps. Após uma correção ou resposta de permissão, volte ao Paws e escolha Verificar novamente. Para suporte e políticas publicadas, use Suporte do App na página do Paws na App Store.",
                    "Reglas de acceso\nDebes tener al menos 16 años en la Unión Europea o 13 en otros lugares. Si eres menor de 18 años, también necesitas permiso de tu madre, padre o tutor legal. Su permiso no sustituye la edad mínima.\n\nPrivacidad\nPaws usa las funciones nativas de edad y permisos de Apple cuando es necesario. Paws no guarda fechas de nacimiento, documentos de identidad ni respuestas de edad de Apple.\n\nAyuda\nPara revisar la información de edad compartida, abre Ajustes → tu cuenta de Apple → Información personal → intervalo de edad para apps. Después de una corrección o respuesta a un permiso, vuelve a Paws y elige Volver a comprobar. Para obtener ayuda y consultar las políticas publicadas, usa Soporte de apps en la página de Paws en el App Store." } },
            };
            foreach (var entry in extra) messages.Add(entry.Key, entry.Value[locale]);
        }
    }
}
