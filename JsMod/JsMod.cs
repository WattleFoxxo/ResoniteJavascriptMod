using HarmonyLib;
using ResoniteModLoader;
using Jint;
using Acornima.Ast;
using System;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.IO;
// using ResoniteHotReloadLib;

namespace JsMod {
	public class JsMod : ResoniteMod {
		internal const string VERSION_CONSTANT = "0.1.0";
		public override string Name => "JsMod";
		public override string Author => "WattleFoxxo (wattle@wattlefoxxo.au)";
		public override string Version => VERSION_CONSTANT;
		public override string Link => "https://wattlefoxxo.au/";

		public static ModConfiguration config;

		[AutoRegisterConfigKey]
		public static ModConfigurationKey<bool> enabled = new ModConfigurationKey<bool>("Enabled", "Enabled/Disable this mod", () => true);
		
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<bool> enableLibraries = new ModConfigurationKey<bool>("Enable Libraries", "Enabled/Disable loading javascript files from the libraries folder", () => true);

		[AutoRegisterConfigKey]
		public static ModConfigurationKey<string> librariesFolder = new ModConfigurationKey<string>("Javascript Libraries Folder", "Location of the javascript libraries folder", () => "./JavascriptLibraries");

		[AutoRegisterConfigKey]
		public static ModConfigurationKey<bool> allowHttpClient = new ModConfigurationKey<bool>("Allow HttpClient", "Allow/Disallow *EXPERIMENTAL* System.Net.Http.HttpClient use in javascript *THIS COULD BE A SECIRITY RISK* *DO NOT CHANGE IF YOU DONT KNOW WHAT YOUR DOING*", () => false);
	
		[AutoRegisterConfigKey]
		public static ModConfigurationKey<bool> allowWebSocketClient = new ModConfigurationKey<bool>("Allow WebSocketClient", "Allow/Disallow *EXPERIMENTAL* ClientWebSocket.ClientWebSocket use in javascript *THIS COULD BE A SECIRITY RISK* *DO NOT CHANGE IF YOU DONT KNOW WHAT YOUR DOING*", () => false);

		const string harmonyId = "au.wattlefoxxo.JsMod";

		public struct Arg {
			string _name;
			object _value;

			public Arg(string name, object value) {
				_name = name;
				_value = value;
			}

			public string name {
				get { return _name; }
				set { _name = value; }
			}

			public object value {
				get { return _value; }
				set { _value = value; }
			}
		}

		public override void OnEngineInit() {
			// HotReloader.RegisterForHotReload(this);

			config = GetConfiguration();
			Setup();
		}
		/*
		static void BeforeHotReload() {
			Harmony harmony = new Harmony(harmonyId);
			harmony.UnpatchAll(harmonyId);
		}

		static void OnHotReload(ResoniteMod modInstance) {
			config = modInstance.GetConfiguration();
			Setup();
		}
		*/
		static void Setup() {
			Harmony harmony = new Harmony(harmonyId);
			harmony.PatchAll();
		}

		public static void LoadJavascriptLibraries(Jint.Engine engine) {
			if (!config.GetValue(enableLibraries)) return;

			string folder = config.GetValue(librariesFolder);

			if (string.IsNullOrEmpty(folder)) return;

			if (!Directory.Exists(folder)) {
				Directory.CreateDirectory(folder);
			}

			string[] files = Directory.GetFiles(folder, "*.js");

			if (files.Length <= 0) return;

			foreach (string file in files) {
				string script = File.ReadAllText(file);

				try {
					engine.Execute(script);
				} catch (Exception ex) {
					Warn($"Exception executing {Path.GetFileName(file)}: {ex.Message}");
				}
			}
		}

		public static void ExecuteJavascript(FrooxEngineContext context, string code, List<Arg> args) {
			Jint.Engine engine = new Jint.Engine();

			engine.SetValue("console", new Console(context));
			engine.SetValue("dynamicImpulseHandler", ProtoFluxHelper.DynamicImpulseHandler);
			engine.SetValue("world", context.World);

			//engine.SetValue("frooxEngine", new {
			//	dynamicImpulseHandler = ProtoFluxHelper.DynamicImpulseHandler,
			//	world = context.World,
			//});

			if (config.GetValue(allowHttpClient))
				engine.SetValue("httpClient", new HttpClient()); // *TODO* evaluate the safty of this class!

			if (config.GetValue(allowWebSocketClient))
				engine.SetValue("webSocketClient", new ClientWebSocket()); // *TODO* evaluate the safty of this class!

			LoadJavascriptLibraries(engine);

			// Set user arguments
			foreach (Arg arg in args) {
				engine.SetValue(arg.name, arg.value);
			}

			engine.Execute(code);
		}

		[HarmonyPatch(typeof(ProtoFlux.Runtimes.Execution.Nodes.Actions.DynamicImpulseTrigger))]
		[HarmonyPatch("Trigger")]
		class TriggerPatch {
			static void Prefix(Slot hierarchy, string tag, bool excludeDisabled, FrooxEngineContext context) {
				if (!config.GetValue(enabled)) return;
				if (hierarchy == null) return;
				if (tag != "_JSMOD_EXECUTE") return;

				try {
					if (tag == "_JSMOD_EXECUTE") {
						Slot codeSlot = hierarchy.GetChildrenWithTag("_JSMOD_CODE").First();
						Slot argsSlot = hierarchy.GetChildrenWithTag("_JSMOD_ARGS").First();

						List<Arg> args = new List<Arg>();
						foreach (FrooxEngine.Component component in argsSlot.Components) {
							if (component.GetType() == typeof(FrooxEngine.DynamicVariableSpace)) continue;

							string name = component.TryGetField("VariableName").ToString();

							if (string.IsNullOrEmpty(name)) continue;

							FrooxEngine.IField valueField = component.TryGetField("Value");
							object value = valueField.BoxedValue;

							if (value != null) {
								args.Add(new Arg(name, value));
							} else {
								Warn($"Value for arg {name} is null.");
								continue;
							}
						}

						string code;						
						DynamicVariableSpace codeSpace = codeSlot.FindSpace("_JSMOD_SPACE");

						codeSpace.TryReadValue("_JSMOD_CODE", out code);

						ExecuteJavascript(context, code, args);
					}
				} catch (Exception ex) {
					Warn($"Exception: {ex}");
				}
			}
		}
	}
}
