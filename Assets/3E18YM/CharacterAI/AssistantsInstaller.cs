using UnityEngine;
using Zenject;
using Sirenix.OdinInspector;

public class AssistantsInstaller : MonoInstaller
{

    // public AssistantType AiModel;
    // public CAI_handler cAI_Handler;
    //
    //
    // public override void InstallBindings()
    // {
    //
    //
    //
    //     Container.Bind<StatusManage>().AsSingle().NonLazy();
    //     Container.Bind<AzureSpeechConfig>().FromInstance(azureConfig.azureSpeechConfig);
    //     Container.Bind<AzureAIConfig>().FromInstance(azureConfig.azureAIConfig);
    //     Container.Bind<mqttConfig>().FromInstance(mqttConfig);
    //
    //     //AI服務
    //     switch (AiModel)
    //     {
    //
    //         case AssistantType.AzureOpenAI:
    //             Container.BindInterfacesTo<AzureAI_Thread>().AsSingle().NonLazy();
    //             break;
    //         case AssistantType.Webduino:
    //             Container.Bind<WebduinoAPI>().AsSingle().NonLazy();
    //             Container.BindInterfacesTo<mqttReceiver>().AsSingle().NonLazy();
    //             break;
    //
    //     }
    //     // Speech 服務
    //     Container.BindInterfacesTo<AzureSTT_Services>().AsSingle().NonLazy();
    //     Container.BindInterfacesTo<AzureTTS_Services>().AsSingle().NonLazy();
    //
    //     Container.Bind<SpeechQueue>().AsSingle().NonLazy();
    //
    //     if (cAI_Handler != null)
    //     {
    //         Container.Bind<CAI_handler>().FromInstance(cAI_Handler).AsSingle();
    //
    //     }
    //
    //
    // }
    // public void ReplaceCAIHandler(CAI_handler newHandler)
    // {
    //     cAI_Handler = newHandler;
    //
    //     // 解除原有綁定並重新綁定新的實例
    //     Container.Unbind<CAI_handler>();
    //     Container.Bind<CAI_handler>().FromInstance(cAI_Handler).AsSingle();
    //
    // }

}



