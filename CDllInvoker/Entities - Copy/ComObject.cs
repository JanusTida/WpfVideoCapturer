using System;
using System.Collections.Generic;
using System.Linq;
using CDllInvoker.StaticMethods;
using System.Runtime.InteropServices;

namespace CDllInvoker.Entities {
    public class ComObject {
        public static ComObject GetLocalObject() {
            bool res = ComObjectMethods.cdfc_devices_init();
            if (res) {
                ComObject comObject = new ComObject();
                List<Device> devices = comObject.Devices;
                List<PhysicsDeviceStruct> partitionInDeviceStructs = new List<PhysicsDeviceStruct>();

                var hddPtr = ComObjectMethods.get_hdd_vender();
                var devicePtr = ComObjectMethods.cdfc_devices_devicelist();
                var partitionPtr = ComObjectMethods.cdfc_devices_patitionlist();

                var deviceNode = devicePtr;
                var partitionNode = partitionPtr;

                while (deviceNode != IntPtr.Zero) {
                    var deviceStructList = deviceNode.GetStructure<DeviceListStruct>();
                    var deviceStruct = deviceStructList.m_ThisDevice.GetStructure<PhysicsDeviceStruct>();
                    if (deviceStruct.ObjectID != 16) {
                        Device device = Device.Create(deviceStruct);
                        for (var hddNode = hddPtr; hddNode != IntPtr.Zero;) {
                            var hddInfoStruct = hddNode.GetStructure<HDDInfoStruct>();
                            if (hddInfoStruct.ID == device.DeviceID) {
                                device.HddInfo = HddInfo.Create(hddInfoStruct);
                            }
                            hddNode = hddInfoStruct.Next;
                        }
                        devices.Add(device);
                    }
                    else {
                        partitionInDeviceStructs.Add(deviceStruct);
                    }
                    deviceNode = deviceStructList.m_next;
                }
                while (partitionNode != IntPtr.Zero) {
                    var ptList = partitionNode.GetStructure<PartitonListStruct>();
                    var partitionStruct = ptList.m_ThisPartition.GetStructure<PartitonStruct>();
                    var device = devices.FirstOrDefault(p => p.DeviceID == partitionStruct.m_LoGo);

                    if (device != null) {
                        var partition = Partition.Create(partitionStruct);
                        partition.Device = device;
                        partition.SectorSize = device.SectorSize;
                        device.Partitions.Add(partition);
                    }
                    partitionNode = ptList.m_next;
                }
                deviceNode = devicePtr;
                devices.ForEach(p => {
                    p.Partitions.ForEach(q => {
                        var devName = @"\\.\" + q.Sign + ":";
                        var partitionInDevice = partitionInDeviceStructs.FirstOrDefault(t => t.DevName == devName);
                        if(partitionInDevice.Handle != null) {
                            q.Handle = partitionInDevice.Handle;
                        }
                    });
                });
                return comObject;
            }
            else {
                throw new Exception("Initializing Error!");
            }
        }
        public void Exit() {
            ComObjectMethods.cdfc_devices_exit();
            ComObjectMethods.exit_hdd_vender();
            ImgFiles.ForEach(p => p.Exit());
        }
        public List<ImgFile> ImgFiles { get; set; }
        public List<Device> Devices { get; set; }
        public ComObject() {
            Devices = new List<Device>();
            ImgFiles = new List<ImgFile>();
        }

    }
    
}
