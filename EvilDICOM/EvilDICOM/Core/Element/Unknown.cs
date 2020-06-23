﻿using System;
using EvilDICOM.Core.Dictionaries;
using EvilDICOM.Core.Enums;
using EvilDICOM.Core.Interfaces;
using EvilDICOM.Core.Logging;

namespace EvilDICOM.Core.Element
{
    /// <summary>
    ///     Encapsulates the Unknown VR type
    /// </summary>
    public class Unknown : AbstractElement<byte>
    {
        public Unknown()
        {
        }

        public Unknown(Tag tag, byte[] data)
            : base(tag, data)
        {
            VR = VR.Unknown;
        }

        /// <summary>
        ///     Used in the try read as method
        /// </summary>
        internal TransferSyntax TransferSyntax { get; set; }

        /// <summary>
        ///     Method used to read out unknown VR types (not in the dictionary).
        /// </summary>
        /// <typeparam name="T">the type of value to try to read out</typeparam>
        /// <param name="outValue">the value read</param>
        /// <param name="tx">the transfer syntax to try (default is Implicit little endian)</param>
        /// <returns>whether or not the read was successfull</returns>
        public bool TryReadAs<T>(out T outValue) where T : IDICOMElement
        {
            VR vr = VRDictionary.GetVRFromType(typeof (T));
            try
            {
                IDICOMElement el = ElementFactory.GenerateElement(Tag, vr, Data_.ToArray(), TransferSyntax);
                outValue = (T) el;
                return true;
            }
            catch (Exception e)
            {
                EvilLogger.Instance.Log("Couldn't cast unknown type as type {0} for {1}", LogPriority.ERROR, typeof (T),
                    Tag);
                outValue = default(T);
                return false;
            }
        }
    }
}