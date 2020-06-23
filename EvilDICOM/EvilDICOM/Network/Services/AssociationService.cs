﻿using System;
using System.Linq;
using EvilDICOM.Network.Associations.PDUs;
using EvilDICOM.Network.DIMSE;
using EvilDICOM.Network.Enums;
using EvilDICOM.Network.Messaging;
using EvilDICOM.Network.PDUs;
using EvilDICOM.Network.Helpers;
using EvilDICOM.Network.Extensions;

namespace EvilDICOM.Network.Services
{
    public class AssociationService
    {
        public AssociationService()
        {
            SetDefaultActions();
        }

        public Action<Abort, Association> AbortReceivedAction { get; set; }
        public Action<Request, Association> AssociationRequestReceivedAction { get; set; }
        public Action<Accept, Association> AssociationAcceptanceReceivedAction { get; set; }
        public Action<ReleaseRequest, Association> ReleaseRequestReceivedAction { get; set; }
        public Action<ReleaseResponse, Association> ReleaseResponseAction { get; set; }
        public Action<Reject, Association> AssociationRejectAction { get; set; }

        public void SetDefaultActions()
        {
            AbortReceivedAction = (abort, asc) =>
            {
                asc.Logger.Log("<-- " + abort);
                RaiseAbortRequestReceived(abort, asc);
                asc.Release();
            };

            AssociationRequestReceivedAction = (req, asc) =>
            {
                asc.Logger.Log("<-- " + req);
                RaiseAssociationRequestReceived(req, asc);
                var ctxs = asc.GetResponseContexts(req.PresentationContexts);
                if (ctxs.Any())
                {
                    var accept = Accept.Generate(req, ctxs);
                    asc.UserInfo = req.UserInfo;
                    asc.State = NetworkState.ASSOCIATION_ESTABLISHED_WAITING_ON_DATA;
                    asc.LastActive = DateTime.Now;
                    asc.PresentationContexts = ctxs; //Simplified agreed contexts
                    AssociationMessenger.SendAccept(accept, asc);
                }
                else
                {
                    asc.State = NetworkState.CLOSING_ASSOCIATION;
                    asc.LastActive = DateTime.Now;
                    AssociationMessenger.SendReject(asc);
                }
            };

            AssociationAcceptanceReceivedAction = (acc, asc) =>
            {
                asc.Logger.Log("<-- " + acc);
                RaiseAssociationAcceptanceReceived(acc, asc);
                asc.SetFinalContexts(acc);
                if (asc.PresentationContexts.Any())
                {
                    asc.UserInfo = acc.UserInfo;

                    asc.State = NetworkState.TRANSPORT_CONNECTION_OPEN;
                    while (asc.OutboundMessages.Any())
                    {
                        if (asc.State == NetworkState.TRANSPORT_CONNECTION_OPEN)
                        {
                            AbstractDIMSEBase dimse;
                            if (asc.OutboundMessages.TryDequeue(out dimse))
                            {
                                PDataMessenger.Send(dimse, asc);
                            }
                        }
                    }
                }
                else
                {
                    asc.Release();
                }              
            };

            AssociationRejectAction = (rej, asc) =>
            {
                asc.Logger.Log("<-- " + rej);
                RaiseAssociationRejectionReceived(rej, asc);
                asc.Release();
            };

            ReleaseRequestReceivedAction = (rel, asc) =>
            {
                asc.Logger.Log("<-- " + rel);
                RaiseReleaseRequestReceived(rel, asc);
                AssociationMessenger.SendReleaseResponse(asc);
                asc.Release();
            };

            ReleaseResponseAction = (rel, asc) =>
            {
                asc.Logger.Log("<-- " + rel);
                RaiseReleaseResponseReceived(rel, asc);
                asc.Release();
            };
        }

        #region EVENTS AND HANDLERS
        public delegate void AbortRequestHandler(Abort abort, Association asc);

        public delegate void AssociationAcceptedHandler(Accept acc, Association asc);

        public delegate void AssociationRejectedHandler(Reject rej, Association asc);

        public delegate void AssociationRequestHandler(Request req, Association asc);

        public delegate void ReleaseRequestHandler(ReleaseRequest relRq, Association asc);

        public delegate void ReleaseResponseHandler(ReleaseResponse relRs, Association asc);

        //ASSOCIATE REQUESTED

        public event AssociationRequestHandler AssociationRequestReceived;

        public void RaiseAssociationRequestReceived(Request req, Association asc)
        {
            if (AssociationRequestReceived != null)
            {
                AssociationRequestReceived(req, asc);
            }
        }

        //ASSOCIATION ACCEPTED

        public event AssociationAcceptedHandler AssociationAcceptanceReceived;

        public void RaiseAssociationAcceptanceReceived(Accept acc, Association asc)
        {
            if (AssociationAcceptanceReceived != null)
            {
                AssociationAcceptanceReceived(acc, asc);
            }
        }

        //ASSOCIATION REJECTED

        public event AssociationRejectedHandler AssociationRejectionReceived;

        public void RaiseAssociationRejectionReceived(Reject rej, Association asc)
        {
            if (AssociationRejectionReceived != null)
            {
                AssociationRejectionReceived(rej, asc);
            }
        }

        //RELEASE REQUEST

        public event ReleaseRequestHandler ReleaseRequestReceived;

        public void RaiseReleaseRequestReceived(ReleaseRequest relReq, Association asc)
        {
            if (ReleaseRequestReceived != null)
            {
                ReleaseRequestReceived(relReq, asc);
            }
        }

        //RELEASE RESPONSE

        public event ReleaseResponseHandler ReleaseResponseReceived;

        public void RaiseReleaseResponseReceived(ReleaseResponse relRs, Association asc)
        {
            if (ReleaseResponseReceived != null)
            {
                ReleaseResponseReceived(relRs, asc);
            }
        }

        //ABORT REQUEST

        public event AbortRequestHandler AbortRequestReceived;

        public void RaiseAbortRequestReceived(Abort abort, Association asc)
        {
            if (AbortRequestReceived != null)
            {
                AbortRequestReceived(abort, asc);
            }
        }

        #endregion
    }
}