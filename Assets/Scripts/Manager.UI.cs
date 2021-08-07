using UnityEngine;
using UnityEngine.UI;

namespace SoundPropagation{
    public partial class Manager{
        private GameObject openUIPanel;
        private GameObject uiPanel;

        private Text averageSoundComputationTime;
        private Text totalSoundComputationTime;
        private Text averageModelComputationTime;
        private Text totalModelComputationTime;
        private Text averageDiffractionComputationTime;
        private Text averageTransmissionComputationTime;
        private Text averageReflectionComputationTime;

        private void initializeUI(){
            openUIPanel = uiObject.transform.GetChild(0).GetChild(0).gameObject;
            uiPanel = uiObject.transform.GetChild(0).GetChild(1).gameObject;

            averageSoundComputationTime = uiPanel.transform.GetChild(1).GetComponent<Text>();
            totalSoundComputationTime = uiPanel.transform.GetChild(2).GetComponent<Text>();
            averageModelComputationTime = uiPanel.transform.GetChild(3).GetComponent<Text>();
            totalModelComputationTime = uiPanel.transform.GetChild(4).GetComponent<Text>();
            
            averageDiffractionComputationTime = uiPanel.transform.GetChild(5).GetComponent<Text>();
            averageTransmissionComputationTime = uiPanel.transform.GetChild(6).GetComponent<Text>();
            averageReflectionComputationTime = uiPanel.transform.GetChild(7).GetComponent<Text>();
        }

        private void updateUI(){
            openUIPanel.SetActive(!Input.GetKey(KeyCode.F1));
            uiPanel.SetActive(!openUIPanel.activeSelf);

            if(debugger.soundModel == null)
                return;

            averageSoundComputationTime.text = "Average Sound Computation Time (ms): " + debugger.soundModel.averageSoundComputationTime.average * 1000;
            totalSoundComputationTime.text = "Total Sound Computation Time (s): " + debugger.soundModel.totalSoundComputationTime.timeElapsed;
            averageModelComputationTime.text = "Average Model Computation Time (ms): " + debugger.soundModel.averageModelComputationTime.average * 1000;
            totalModelComputationTime.text = "Total Model Computation Time (s): " + debugger.soundModel.totalModelComputationTime.timeElapsed;
            
            averageDiffractionComputationTime.text = "Average Diffraction Computation Time (ms): " + debugger.soundModel.averageDiffractionSoundComputationTime.average * 1000;
            averageTransmissionComputationTime.text = "Average Transmission Computation Time (ms): " + debugger.soundModel.averageTransmissionSoundComputationTime.average * 1000;
            averageReflectionComputationTime.text = "Average Reflection Computation Time (ms): " + debugger.soundModel.averageReflectionSoundComputationTime.average * 1000;
        }
    }
}