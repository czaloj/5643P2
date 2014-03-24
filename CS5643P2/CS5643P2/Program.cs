using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CS5643P2 {
    class Program : Game {
        GraphicsDeviceManager graphics;

        public Program() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();
        }

        // Soft Body And Rendering Info
        SoftBody sbc;
        VertexPositionTexture[] verts;
        int[] inds;

        List<Constraint> constraints;

        BasicEffect fx;
        Texture2D t;
        float a = 0;

        protected override void Initialize() {
            base.Initialize();
        }
        protected override void LoadContent() {
            // Read Soft Body
            VertexPositionNormalTexture[] vb;
            using(var s = File.OpenRead("Cloth.obj")) {
                sbc = SoftBody.Parse(s, out vb, out inds);
            }
            verts = new VertexPositionTexture[vb.Length];
            for(int i = 0; i < verts.Length; i++) {
                verts[i] = new VertexPositionTexture(vb[i].Position, vb[i].TextureCoordinate);
            }

            // Make Spring Constraints
            constraints = new List<Constraint>();
            SpringConstraint sc;
            for(int i = 0; i < sbc.tris.Length; i++) {
                sc = new SpringConstraint(sbc, sbc.tris[i].P1, sbc, sbc.tris[i].P2);
                sc.K = 80; sc.Stiffness = 1f;
                constraints.Add(sc);
                sc = new SpringConstraint(sbc, sbc.tris[i].P1, sbc, sbc.tris[i].P3);
                sc.K = 80; sc.Stiffness = 1f;
                constraints.Add(sc);
                sc = new SpringConstraint(sbc, sbc.tris[i].P2, sbc, sbc.tris[i].P3);
                sc.K = 80; sc.Stiffness = 1f;
                constraints.Add(sc);
            }

            Random r = new Random();
            for(int i = 0; i < sbc.positions.Length; i++) {
                sbc.positions[i].Y += (r.Next(0, 80) - 40) * 0.02f;
            }

            // Create Visualization Texture
            int h = 17, w = h << 1;
            t = new Texture2D(GraphicsDevice, w, h);
            Color[] c = new Color[w * h];
            int ti = 0;
            for(int x = 0; x < h; x++) {
                for(int y = 0; y < h; y++) {
                    c[y * w + x] = (ti % 2 == 0) ? Color.Red : Color.Orange;
                    c[y * w + x + h] = (ti % 2 == 0) ? Color.Blue : Color.Cyan;
                    ti++;
                }
            }
            t.SetData(c);

            // Cloth Shader
            fx = new BasicEffect(GraphicsDevice);
            fx.TextureEnabled = true;
            fx.VertexColorEnabled = false;
            fx.LightingEnabled = false;
            fx.FogEnabled = false;
            fx.World = Matrix.Identity;
            fx.View = Matrix.CreateLookAt(new Vector3(12, 3, 12), Vector3.Zero, Vector3.Up);
            fx.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, 1000f);
            fx.DiffuseColor = Vector3.One;
            fx.Texture = t;

            base.LoadContent();
        }
        protected override void UnloadContent() {
            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            a += dt;
            a = MathHelper.WrapAngle(a);

            for(int i = 0; i < constraints.Count; i++) {
                constraints[i].Apply(dt);
            }

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            for(int i = 0; i < verts.Length; i++)
                verts[i].Position = sbc.positions[i];

            GraphicsDevice.Clear(Color.Transparent);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

            fx.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length, inds, 0, inds.Length / 3, VertexPositionTexture.VertexDeclaration);

            GraphicsDevice.SetVertexBuffers(null);
            GraphicsDevice.Indices = null;

            base.Draw(gameTime);
        }

        static void Main() {
            using(Program p = new Program()) {
                p.Run();
            }
        }
    }
}