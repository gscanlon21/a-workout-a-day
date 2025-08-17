﻿using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Exercise.Skills;
using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Data.Query.Options;
using System.Linq.Expressions;
using System.Reflection;

namespace Data.Query;

public interface IExerciseVariationCombo
{
    Exercise Exercise { get; }
    Variation Variation { get; }
}

public static class Filters
{
    /// <summary>
    ///     Filter exercises to ones that help with a specific sport
    /// </summary>
    /// <param name="sportsFocus">
    ///     If null, does not filter the query.
    ///     If SportsFocus.None, filters the query down to exercises that don't target a sport.
    ///     If > SportsFocus.None, filters the query down to exercises that target that specific sport.
    /// </param>
    public static IQueryable<T> FilterSportsFocus<T>(IQueryable<T> query, SportsFocus? sportsFocus, bool includeNone = false) where T : IExerciseVariationCombo
    {
        if (sportsFocus.HasValue && sportsFocus != SportsFocus.None)
        {
            if (includeNone)
            {
                query = query.Where(i =>
                    i.Variation.SportsFocus.HasFlag(sportsFocus.Value)
                    || i.Variation.SportsFocus == SportsFocus.None
                );
            }
            else
            {
                query = query.Where(i => i.Variation.SportsFocus.HasFlag(sportsFocus.Value));
            }
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise is for the correct workout section. HasAnyFlag logic.
    /// </summary>
    public static IQueryable<T> FilterSection<T>(IQueryable<T> query, Section? value) where T : IExerciseVariationCombo
    {
        // Debug should be able to see all exercises.
        if (value.HasValue && value != Section.None && value != Section.Debug)
        {
            // Has any flag.
            query = query.Where(vm => (vm.Variation.Section & value.Value) != 0);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise is for the correct workout type
    /// </summary>
    public static IQueryable<T> FilterExerciseFocus<T>(IQueryable<T> query, IList<ExerciseFocus>? value, bool exclude = false) where T : IExerciseVariationCombo
    {
        if (value != null)
        {
            if (exclude)
            {
                query = query.Where(vm => !value.Any(v => vm.Variation.ExerciseFocus.HasFlag(v)));
            }
            else
            {
                query = query.Where(vm => value.Any(v => vm.Variation.ExerciseFocus.HasFlag(v)));
            }
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterMuscleMovement<T>(IQueryable<T> query, MuscleMovement? muscleMovement) where T : IExerciseVariationCombo
    {
        if (muscleMovement.HasValue)
        {
            // Has any flag
            query = query.Where(vm => (vm.Variation.MuscleMovement & muscleMovement.Value) != 0);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterMovementPattern<T>(IQueryable<T> query, MovementPattern? muscleMovement) where T : IExerciseVariationCombo
    {
        if (muscleMovement.HasValue)
        {
            if (muscleMovement == MovementPattern.None)
            {
                query = query.Where(vm => vm.Variation.MovementPattern == MovementPattern.None);
            }
            else
            {
                // Has any flag
                query = query.Where(vm => (vm.Variation.MovementPattern & muscleMovement.Value) != 0);
            }
        }

        return query;
    }

    /// <summary>
    /// Filter down to these specific exercises
    /// </summary>
    public static IQueryable<T> FilterExercises<T>(IQueryable<T> query, IList<int>? exerciseIds) where T : IExerciseVariationCombo
    {
        if (exerciseIds != null)
        {
            query = query.Where(vm => exerciseIds.Contains(vm.Exercise.Id));
        }

        return query;
    }

    /// <summary>
    /// Filter down to these specific exercises
    /// </summary>
    public static IQueryable<T> FilterVariations<T>(IQueryable<T> query, IList<int>? variationIds) where T : IExerciseVariationCombo
    {
        if (variationIds != null)
        {
            query = query.Where(vm => variationIds.Contains(vm.Variation.Id));
        }

        return query;
    }

    /// <summary>
    /// Filters exercises to those that work specific skills.
    /// </summary>
    public static IQueryable<T> FilterSkills<T>(IQueryable<T> query, SkillsOptions options) where T : IExerciseVariationCombo
    {
        if (options.HasData())
        {
            if (options.VocalSkills != VocalSkills.None)
            {
                // Has any flag.
                query = query.Where(i => (!options.RequireSkills && i.Exercise.VocalSkills == 0) || (i.Exercise.VocalSkills & options.VocalSkills) != 0);
            }

            if (options.VisualSkills != VisualSkills.None)
            {
                // Has any flag.
                query = query.Where(i => (!options.RequireSkills && i.Exercise.VisualSkills == 0) || (i.Exercise.VisualSkills & options.VisualSkills) != 0);
            }

            if (options.CervicalSkills != CervicalSkills.None)
            {
                // Has any flag.
                query = query.Where(i => (!options.RequireSkills && i.Exercise.CervicalSkills == 0) || (i.Exercise.CervicalSkills & options.CervicalSkills) != 0);
            }

            if (options.ThoracicSkills != ThoracicSkills.None)
            {
                // Has any flag.
                query = query.Where(i => (!options.RequireSkills && i.Exercise.ThoracicSkills == 0) || (i.Exercise.ThoracicSkills & options.ThoracicSkills) != 0);
            }

            if (options.LumbarSkills != LumbarSkills.None)
            {
                // Has any flag.
                query = query.Where(i => (!options.RequireSkills && i.Exercise.LumbarSkills == 0) || (i.Exercise.LumbarSkills & options.LumbarSkills) != 0);
            }
        }

        return query;
    }

    /// <summary>
    /// Filters exercises to whether they use certain equipment.
    /// </summary>
    public static IQueryable<T> FilterEquipment<T>(IQueryable<T> query, Equipment? equipment) where T : IExerciseVariationCombo
    {
        if (equipment.HasValue)
        {
            if (equipment == Equipment.None)
            {
                query = query.Where(i => i.Variation.Instructions.Where(i => i.DisabledReason == null).All(i => i.Equipment == Equipment.None));
            }
            else
            {
                // Has any flag
                query = query.Where(i => i.Variation.Instructions.Where(i => i.DisabledReason == null).Any(i => (i.Equipment & equipment) != 0));
            }
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise works a specific muscle group
    /// </summary>
    public static IQueryable<T> FilterMuscleGroup<T>(IQueryable<T> query, MusculoskeletalSystem? muscleGroup, bool include, Expression<Func<IExerciseVariationCombo, MusculoskeletalSystem>> muscleTarget) where T : IExerciseVariationCombo
    {
        if (muscleGroup.HasValue && muscleGroup != MusculoskeletalSystem.None)
        {
            if (include)
            {
                query = WithMuscleTarget(query, muscleTarget, muscleGroup.Value, include);
            }
            else
            {
                // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                query = WithMuscleTarget(query, muscleTarget, muscleGroup.Value, include);
            }
        }

        return query;
    }

    /// <summary>
    /// Builds an expression consumable by EF Core for filtering what muscles a variation works.
    /// </summary>
    private static IQueryable<T> WithMuscleTarget<T>(this IQueryable<T> entities,
        Expression<Func<IExerciseVariationCombo, MusculoskeletalSystem>> propertySelector, MusculoskeletalSystem muscleGroup, bool include)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(T));

        // Has any flag
        var innerExpr = new MuscleGroupsExpressionRewriter(parameter).Modify(propertySelector);
        var expression = Expression.Lambda<Func<T, bool>>(
            Expression.Equal(Expression.NotEqual(
            Expression.And(
                Expression.Convert(innerExpr, typeof(long)),
                Expression.Convert(Expression.Constant(muscleGroup), typeof(long))
            ),
            Expression.Constant(0L)), Expression.Constant(include)),
            parameter);

        return entities.Where(expression);
    }

    /// <summary>
    /// Re-writes a C# muscle target expression to be consumable by EF Core.
    /// ev => ev.Variation.Strengthens | ev.Variations.Stretches...
    /// </summary>
    private class MuscleGroupsExpressionRewriter(ParameterExpression parameter) : ExpressionVisitor
    {
        /// <summary>
        /// The IExerciseVariationCombo
        /// </summary>
        public ParameterExpression Parameter { get; } = parameter;

        public Expression Modify(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            // vm => Convert(Convert(vm.Variation.Strengthens, Int32) | Convert(vm.Variation.Stretches, Int32), Int32)
            return Visit(node.Body);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // vm.Variation.Strengthens
            var innerProp = Expression.Property(Parameter, (PropertyInfo)((MemberExpression)node.Expression!).Member);
            return Expression.Property(innerProp, (PropertyInfo)node.Member);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert)
            {
                // Convert(vm.Variation.Strengthens, Int32)
                var innerExpr = Visit(node.Operand);
                return Expression.Convert(innerExpr, typeof(long));
            }

            throw new InvalidOperationException();
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            // Convert(vm.Variation.Strengthens, Int32) | Convert(vm.Variation.Stretches, Int32)
            var leftExpr = Visit(node.Left);
            var rightExpr = Visit(node.Right);

            return node.NodeType switch
            {
                ExpressionType.Or => Expression.Or(leftExpr, rightExpr),
                _ => throw new InvalidOperationException(),
            };
        }
    }
}
