using Application.DTO;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public class SignalMapping : Profile
    {

        public SignalMapping()
        {
            CreateMap<GlobalSignalDTO, Signal>().ForMember(dest => dest.Asset, opts => opts.Ignore()).
                ForMember(dest => dest.AssetId, opts => opts.Ignore());

        }

    }
}
