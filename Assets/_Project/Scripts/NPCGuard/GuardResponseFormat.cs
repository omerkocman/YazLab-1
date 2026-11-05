using System;

[Serializable]
internal class GuardResponseFormat
{
    public string type = "json_schema";
    public SchemaOptions json_schema = new SchemaOptions();

    [Serializable]
    public class SchemaOptions
    {
        public string name = "npc_guard_response";
        public bool strict = true;
        public GuardSchema schema = new GuardSchema();
    }

    [Serializable]
    public class GuardSchema
    {
        public string type = "object";
        public GuardProperties properties = new GuardProperties();
        public string[] required = { "response", "openDoor" };
        public bool additionalProperties = false;
    }

    [Serializable]
    public class GuardProperties
    {
        public FieldType response = new FieldType { type = "string" };
        public FieldType openDoor = new FieldType { type = "boolean" };
    }

    [Serializable]
    public class FieldType
    {
        public string type;
    }
}
