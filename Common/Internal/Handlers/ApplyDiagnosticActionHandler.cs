﻿using System.Threading;
using System.Threading.Tasks;
using MirrorSharp.Advanced;
using MirrorSharp.Internal.Results;

namespace MirrorSharp.Internal.Handlers {
    internal class ApplyDiagnosticActionHandler : ICommandHandler {
        public char CommandId => CommandIds.ApplyDiagnosticAction;

        public async Task ExecuteAsync(AsyncData data, WorkSession session, ICommandResultSender sender, CancellationToken cancellationToken) {
            var roslynSession = session.Roslyn;

            var actionId = FastConvert.Utf8ByteArrayToInt32(data.GetFirst());
            var action = roslynSession.CurrentCodeActions[actionId];

            var operations = await action.GetOperationsAsync(cancellationToken).ConfigureAwait(false);
            foreach (var operation in operations) {
                operation.Apply(roslynSession.Workspace, cancellationToken);
            }
            // I rollback the changes since I want to send them to client and get them back as ReplaceText
            // This makes sure any other typing on client merges with these changes properly
            var changes = await roslynSession.RollbackWorkspaceChangesAsync().ConfigureAwait(false);

            var writer = sender.StartJsonMessage("changes");
            writer.WriteProperty("reason", "fix");
            writer.WritePropertyStartArray("changes");
            foreach (var change in changes) {
                writer.WriteChange(change);
            }
            writer.WriteEndArray();
            await sender.SendJsonMessageAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
