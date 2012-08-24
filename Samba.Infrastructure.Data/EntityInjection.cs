using System;
using System.Collections;
using System.Linq;
using Omu.ValueInjecter;

namespace Samba.Infrastructure.Data
{
    public class EntityInjection : ConventionInjection
    {
        protected override bool Match(ConventionInfo c)
        {
            var propertyMatch = c.SourceProp.Name == c.TargetProp.Name;
            var sourceNotNull = c.SourceProp.Value != null;

            var targetPropertyIdWritable = true;

            if (propertyMatch && c.TargetProp.Name == "Id" && !(c.Target.Value is IEntity))
                targetPropertyIdWritable = false;

            return propertyMatch && sourceNotNull && targetPropertyIdWritable;
        }

        protected override object SetValue(ConventionInfo c)
        {
            if (c.SourceProp.Type.IsValueType || c.SourceProp.Type == typeof(string))
                return c.SourceProp.Value;

            if (c.SourceProp.Type.IsGenericType)
            {
                var td = c.SourceProp.Type.GetGenericTypeDefinition();
                if (td != null && td.GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var targetChildType = c.TargetProp.Type.GetGenericArguments()[0];
                    if (targetChildType.IsValueType || targetChildType == typeof(string)) return c.SourceProp.Value;
                    if (targetChildType.GetInterfaces().Any(x => x == typeof(IValue)))
                    {
                        var deleteMethod = c.TargetProp.Value.GetType().GetMethod("Remove");
                        //var removingItems =
                        //(from vl in (c.TargetProp.Value as IEnumerable).Cast<IValue>()
                        // let srcv = (c.SourceProp.Value as IEnumerable).Cast<IValue>().SingleOrDefault(z => z.Id == vl.Id)
                        // where srcv == null
                        // select vl).ToList();
                        var rmvItems = (c.TargetProp.Value as IEnumerable).Cast<IValue>()
                            .Where(x => x.Id > 0 && !(c.SourceProp.Value as IEnumerable).Cast<IValue>().Any(y => y.Id == x.Id));
                        rmvItems.ToList().ForEach(x => deleteMethod.Invoke(c.TargetProp.Value, new[] { x }));
                        rmvItems = (c.TargetProp.Value as IEnumerable).Cast<IValue>()
                            .Where(x => !(c.SourceProp.Value as IEnumerable).Cast<IValue>().Contains(x));
                        rmvItems.ToList().ForEach(x => deleteMethod.Invoke(c.TargetProp.Value, new[] { x }));

                        var sourceCollection = (c.SourceProp.Value as IEnumerable).Cast<IValue>();

                        foreach (var s in sourceCollection)
                        {
                            var sv = s;
                            var target = (c.TargetProp.Value as IEnumerable).Cast<IValue>().SingleOrDefault(z => z.Id == sv.Id && z.Id != 0);
                            if (target != null) target.InjectFrom<EntityInjection>(sv);
                            else if (!(c.TargetProp.Value as IEnumerable).Cast<IValue>().Contains(sv))
                            {
                                var addMethod = c.TargetProp.Value.GetType().GetMethod("Add");
                                addMethod.Invoke(c.TargetProp.Value, new[] { sv });
                            }
                        }
                    }
                }

                return c.TargetProp.Value;
            }

            if (c.TargetProp.Value == null)
                c.TargetProp.Value = Activator.CreateInstance(c.TargetProp.Type);

            return c.TargetProp.Value.InjectFrom<EntityInjection>(c.SourceProp.Value);
        }
    }
}