using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BossEnemyDataSO))]
public class BossEnemyDataSOEditor : Editor
{
    SerializedProperty idleBetweenSkillsProp;
    SerializedProperty skillsProp;
    SerializedProperty fireballPrefabProp;
    SerializedProperty baseYawForLeftProp;

    void OnEnable()
    {
        idleBetweenSkillsProp = serializedObject.FindProperty("idleBetweenSkills");
        skillsProp = serializedObject.FindProperty("skills");
        fireballPrefabProp = serializedObject.FindProperty("fireballPrefab");
        baseYawForLeftProp = serializedObject.FindProperty("baseYawForLeft");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Loop", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(idleBetweenSkillsProp);

        EditorGUILayout.Space(8);
        DrawSkills();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Projectiles", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(fireballPrefabProp);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Facing", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(baseYawForLeftProp);

        serializedObject.ApplyModifiedProperties();
    }

    void DrawSkills()
    {
        EditorGUILayout.LabelField("Skills", EditorStyles.boldLabel);

        if (skillsProp == null)
        {
            EditorGUILayout.HelpBox("skills 필드를 찾을 수 없습니다. BossEnemyDataSO의 필드명이 skills인지 확인하세요.", MessageType.Error);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Size: {skillsProp.arraySize}");
        if (GUILayout.Button("+", GUILayout.Width(30))) skillsProp.arraySize++;
        if (GUILayout.Button("-", GUILayout.Width(30)) && skillsProp.arraySize > 0) skillsProp.arraySize--;
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < skillsProp.arraySize; i++)
        {
            SerializedProperty entry = skillsProp.GetArrayElementAtIndex(i);

            using (new EditorGUILayout.VerticalScope("box"))
            {
                SerializedProperty typeProp = entry.FindPropertyRelative("type");
                SerializedProperty weightProp = entry.FindPropertyRelative("weight");

                EditorGUILayout.PropertyField(typeProp);
                EditorGUILayout.PropertyField(weightProp);

                BossSkillType type = (BossSkillType)typeProp.enumValueIndex;

                EditorGUILayout.Space(4);

                switch (type)
                {
                    case BossSkillType.RandomMoveWait:
                        Draw(entry,
                            "randomMoveSpeed",
                            "randomMoveDistanceMin",
                            "randomMoveDistanceMax",
                            "randomMoveAfterIdle"
                        );
                        break;

                    case BossSkillType.RandomJumpMove:
                        Draw(entry,
                            "jumpDistanceMin",
                            "jumpDistanceMax",
                            "jumpDuration",
                            "jumpApexHeightMin",
                            "jumpApexHeightPerUnit"
                        );
                        break;

                    case BossSkillType.DashToPlayerDir:
                        Draw(entry,
                            "dashSpeed"
                        );
                        break;

                    case BossSkillType.UndergroundDoubleFire:
                        Draw(entry,
                            "undergroundWalkDeltaZ",
                            "undergroundWalkSpeed",
                            "undergroundEnterTime",
                            "undergroundDropY",
                            "undergroundDropTime",
                            "undergroundBeforeFireWait",
                            "fireHorizontalSpeed",
                            "undergroundAfterFireWait",
                            "undergroundRiseTime",
                            "undergroundExitTime"
                        );
                        break;

                    case BossSkillType.TwoShotsToPlayerPos:
                        Draw(entry,
                            "aimShotSpeed",
                            "aimShotInterval",
                            "aimShotCount"
                        );
                        break;
                }

                EditorGUILayout.Space(6);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Up") && i > 0) skillsProp.MoveArrayElement(i, i - 1);
                if (GUILayout.Button("Down") && i < skillsProp.arraySize - 1) skillsProp.MoveArrayElement(i, i + 1);
                if (GUILayout.Button("Remove"))
                {
                    skillsProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    void Draw(SerializedProperty entry, params string[] names)
    {
        foreach (var n in names)
        {
            var p = entry.FindPropertyRelative(n);
            if (p != null)
                EditorGUILayout.PropertyField(p);
        }
    }
}
