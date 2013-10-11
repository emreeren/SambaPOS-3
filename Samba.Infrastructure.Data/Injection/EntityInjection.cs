using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using Omu.ValueInjecter;

namespace Samba.Infrastructure.Data.Injection
{
    public class EntityInjection : ConventionInjection
    {
        protected override bool Match(ConventionInfo c)
        {
            var propertyMatch = c.SourceProp.Name == c.TargetProp.Name;
            var sourceNotNull = c.SourceProp.Value != null;

            bool targetPropertyIdWritable = !(propertyMatch && c.TargetProp.Name == "Id" && !(c.Target.Value is IEntityClass));

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
                    if (targetChildType.GetInterfaces().Any(x => x == typeof(IValueClass)))
                    {
                        var deleteMethod = c.TargetProp.Value.GetType().GetMethod("Remove");
                        var rmvItems = (c.TargetProp.Value as IEnumerable).Cast<IValueClass>()
                            .Where(x => x.Id > 0 && !(c.SourceProp.Value as IEnumerable).Cast<IValueClass>().Any(y => y.Id == x.Id));
                        rmvItems.ToList().ForEach(x => deleteMethod.Invoke(c.TargetProp.Value, new[] { x }));
                        rmvItems = (c.TargetProp.Value as IEnumerable).Cast<IValueClass>()
                            .Where(x => !(c.SourceProp.Value as IEnumerable).Cast<IValueClass>().Contains(x));
                        rmvItems.ToList().ForEach(x => deleteMethod.Invoke(c.TargetProp.Value, new[] { x }));

                        var sourceCollection = (c.SourceProp.Value as IEnumerable).Cast<IValueClass>();

                        foreach (var s in sourceCollection)
                        {
                            var sv = s;

                            var target = (c.TargetProp.Value as IEnumerable).Cast<IValueClass>().SingleOrDefault(z => z.Id == sv.Id && z.Id != 0);
                            if (target != null) target.InjectFrom<EntityInjection>(sv);
                            else if (!(c.TargetProp.Value as IEnumerable).Cast<IValueClass>().Contains(sv))
                            {
                                if (!(sv is IEntityClass))
                                {
                                    sv = Activator.CreateInstance(targetChildType) as IValueClass;
                                    Debug.Assert(sv != null);
                                    sv.InjectFrom<EntityInjection>(s);
                                    sv.Id = 0;
                                }

                                var addMethod = c.TargetProp.Value.GetType().GetMethod("Add");
                                addMethod.Invoke(c.TargetProp.Value, new[] { sv });
                            }
                        }
                    }
                }

                return c.TargetProp.Value;
            }

            if (c.SourceProp.Value is IEntityClass)
            {
                return c.SourceProp.Value;
            }

            if (c.TargetProp.Value == null)
            {
                try
                {
                    c.TargetProp.Value = Activator.CreateInstance(c.TargetProp.Type);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return c.TargetProp.Value.InjectFrom<EntityInjection>(c.SourceProp.Value);
        }
    }
}