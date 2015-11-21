using System.Collections.Generic;
using System.Linq;
using Extreme.Core;

namespace Extreme.Cartesian.Model
{
    public static class TranceiverUtils
    {
        public static Transceiver[] CreateAnomalyToAnomaly(IAnomaly anomaly)
            => Combine(CreateAnomalyReceivers(anomaly), 
                       CreateAnomalyTransmitters(anomaly));

        public static Transceiver[] CreateAnomalyToAnomalySymm(IAnomaly anomaly)
            => CombineSymm(CreateAnomalyReceivers(anomaly), 
                           CreateAnomalyTransmitters(anomaly));

        public static Transceiver[] CreateSourceToAnomaly(IAnomaly anomaly, decimal thickness,
            params SourceLayer[] srcLayers)
            => Combine(CreateAnomalyReceivers(anomaly), 
               CreateSourceTransmitters(thickness, srcLayers));

        public static Transceiver[] CreateSourceToObservations(decimal thickness, SourceLayer layer,
            params ObservationLevel[] obsLevels)
            => Combine(CreateObservationReceivers(obsLevels),
                       CreateSourceTransmitters(thickness, layer));

        public static Transceiver[] CreateAnomalyToObservation(IAnomaly anomaly, params ObservationLevel[] obsLevels)
                => Combine(CreateObservationReceivers(obsLevels),
                           CreateAnomalyTransmitters(anomaly));

        public static Transceiver[] CreateAnomalyToObservation(IAnomaly anomaly, params ObservationSite[] obsSites)
               => Combine(CreateObservationReceivers(obsSites),
                           CreateAnomalyTransmitters(anomaly));

        private static IEnumerable<Transceiver> CreateAnomalyToLevels(IAnomaly anomaly, decimal[] levels)
        {
            var receivers = new List<Receiver>();

            for (int i = 0; i < levels.Length; i++)
                receivers.Add(Receiver.NewFlat(levels[i], i));

            var transmitters = CreateAnomalyTransmitters(anomaly);

            var result =
                from rc in receivers
                from tr in transmitters
                select new Transceiver(tr, rc);

            return result;
        }



        private static Transceiver[] Combine(Receiver[] receivers, Transmitter[] transmitters)
        {
            var result =
                from rc in receivers
                from tr in transmitters
                select new Transceiver(tr, rc);

            return result.ToArray();
        }

        private static Transceiver[] CombineSymm(Receiver[] receivers, Transmitter[] transmitters)
        {
            var result =
                from tr in transmitters
                from rc in receivers
                where rc.Index <= tr.Index
                select new Transceiver(tr, rc);

            return result.ToArray();
        }

        private static Transmitter[] CreateSourceTransmitters(decimal thickness, params SourceLayer[] srcLayers)
        {
            var transmitters = new List<Transmitter>();

            for (int i = 0; i < srcLayers.Length; i++)
                transmitters.Add(Transmitter.NewVolumetric(srcLayers[i].Z - thickness / 2, thickness, i));

            return transmitters.ToArray();
        }

        private static Receiver[] CreateObservationReceivers(params ObservationLevel[] obsLevels)
        {
            var receivers = new List<Receiver>();

            for (int i = 0; i < obsLevels.Length; i++)
                receivers.Add(Receiver.NewFlat(obsLevels[i].Z, i));

            return receivers.ToArray();;
        }

        private static Receiver[] CreateObservationReceivers(params ObservationSite[] obsSite)
        {
            var receivers = new List<Receiver>();

            for (int i = 0; i < obsSite.Length; i++)
                receivers.Add(Receiver.NewFlat(obsSite[i].Z, i));

            return receivers.ToArray(); ;
        }

        private static Transmitter[] CreateAnomalyTransmitters(IAnomaly anomaly)
        {
            var transmitters = new List<Transmitter>();

            for (int i = 0; i < anomaly.Layers.Count; i++)
            {
                var al = anomaly.Layers[i];
                transmitters.Add(Transmitter.NewVolumetric(al.Depth, al.Thickness, i));
            }

            return transmitters.ToArray();
        }


        private static Receiver[] CreateAnomalyReceivers(IAnomaly anomaly)
        {
            var receivers = new List<Receiver>();

            for (int i = 0; i < anomaly.Layers.Count; i++)
            {
                var al = anomaly.Layers[i];
                receivers.Add(Receiver.NewVolumetric(al.Depth, al.Thickness, i));
            }

            return receivers.ToArray();
        }
    }
}
