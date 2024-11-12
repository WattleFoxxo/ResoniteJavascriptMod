using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Elements.Core;

using FrooxEngine.ProtoFlux.Runtimes.Execution.Nodes.Actions;
using FrooxEngine.ProtoFlux;
using FrooxEngine;

namespace JsMod {

	public class Console {
		private FrooxEngineContext _context;

		public Console(FrooxEngineContext context) {
			_context = context;
		}

		public void Debug(params object[] args) {
			string message = string.Join(" ", args);

			JsMod.Msg($"[Javascript Console] [Debug] {message}");
			ProtoFluxHelper.DynamicImpulseHandler.TriggerDynamicImpulseWithArgument<string>(_context.World.RootSlot, "_JSMOD_CONSOLE_DEBUG", false, message.ToString());
		}
		public void log(params object[] args) {
			string message = string.Join(" ", args);

			JsMod.Msg($"[Javascript Console] [Log] {message}");
			ProtoFluxHelper.DynamicImpulseHandler.TriggerDynamicImpulseWithArgument<string>(_context.World.RootSlot, "_JSMOD_CONSOLE_LOG", false, message.ToString());
		}

		public void Info(params object[] args) {
			string message = string.Join(" ", args);

			JsMod.Msg($"[Javascript Console] [Info] {message}");
			ProtoFluxHelper.DynamicImpulseHandler.TriggerDynamicImpulseWithArgument<string>(_context.World.RootSlot, "_JSMOD_CONSOLE_INFO", false, message.ToString());
		}

		public void Warn(params object[] args) {
			string message = string.Join(" ", args);

			JsMod.Msg($"[Javascript Console] [Warn] {message}");
			ProtoFluxHelper.DynamicImpulseHandler.TriggerDynamicImpulseWithArgument<string>(_context.World.RootSlot, "_JSMOD_CONSOLE_WARN", false, message.ToString());
		}

		public void Error(params object[] args) {
			string message = string.Join(" ", args);

			JsMod.Msg($"[Javascript Console] [Error] {message}");
			ProtoFluxHelper.DynamicImpulseHandler.TriggerDynamicImpulseWithArgument<string>(_context.World.RootSlot, "_JSMOD_CONSOLE_ERROR", false, message.ToString());
		}
	}
}
