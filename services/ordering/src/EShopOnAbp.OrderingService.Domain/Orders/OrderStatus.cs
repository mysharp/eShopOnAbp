﻿using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;

namespace EShopOnAbp.OrderingService.Orders;

public class OrderStatus : Enumeration
{
    public static OrderStatus Placed = new OrderStatus(1, nameof(Placed).ToLowerInvariant());
    public static OrderStatus Paid = new OrderStatus(2, nameof(Paid).ToLowerInvariant());
    public static OrderStatus Shipped = new OrderStatus(3, nameof(Shipped).ToLowerInvariant());
    public static OrderStatus Cancelled = new OrderStatus(4, nameof(Cancelled).ToLowerInvariant());

    public OrderStatus(int id, string name) : base(id, name)
    {
    }

    public static IEnumerable<OrderStatus> List() =>
        new[] {Placed, Paid, Shipped, Cancelled};

    public static OrderStatus FromName(string name)
    {
        var state = List()
            .SingleOrDefault(s => String.Equals(s.Name, name, StringComparison.CurrentCultureIgnoreCase));

        if (state == null)
        {
            throw new BusinessException(OrderingServiceErrorCodes.OrderingStatusNotFound)
                .WithData("OrderStatus", String.Join(",", List().Select(s => s.Name)));
        }

        return state;
    }

    public static OrderStatus From(int id)
    {
        var state = List().SingleOrDefault(s => s.Id == id);

        if (state == null)
        {
            throw new BusinessException(OrderingServiceErrorCodes.OrderingStatusNotFound)
                .WithData("OrderStatus", String.Join(",", List().Select(s => s.Name)));
        }

        return state;
    }
}